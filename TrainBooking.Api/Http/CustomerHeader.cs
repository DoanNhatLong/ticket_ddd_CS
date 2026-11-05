using System.Globalization;

namespace TrainBooking.Api.Http;

/// <summary>
/// Temporary customer identification until real authentication exists:
/// the caller sends the customer's user id in the <c>X-Customer-Id</c> header.
/// </summary>
public static class CustomerHeader
{
    public const string Name = "X-Customer-Id";

    /// <summary>
    /// Returns the customer id from the request, or <c>null</c> when the header is missing or not a positive integer.
    /// </summary>
    public static int? Read(HttpContext httpContext)
    {
        var raw = httpContext.Request.Headers[Name].ToString();

        return int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0
            ? id
            : null;
    }
}
