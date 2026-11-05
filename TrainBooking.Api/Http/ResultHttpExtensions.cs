using Microsoft.AspNetCore.Mvc;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Api.Http;

/// <summary>
/// Maps business-rule failures (<see cref="Error"/>) to HTTP problem responses.
/// </summary>
public static class ResultHttpExtensions
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest
    };

    /// <summary>
    /// Writes <paramref name="error"/> as an RFC 7807 problem response whose <c>code</c> extension
    /// carries the stable error code.
    /// </summary>
    public static Task SendErrorAsync(this HttpContext httpContext, Error error, CancellationToken cancellationToken)
    {
        var statusCode = error.Type.ToStatusCode();

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description
        };
        problem.Extensions["code"] = error.Code;

        httpContext.Response.StatusCode = statusCode;
        return httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
    }
}
