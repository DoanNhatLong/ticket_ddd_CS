using System.Reflection;
using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Application.Abstractions.Behaviors;

/// <summary>
/// Builds a failed <see cref="Result"/> or <see cref="Result{TValue}"/> for a pipeline behavior's response type.
/// </summary>
internal static class ResultFailureFactory
{
    private static readonly MethodInfo GenericFailure = typeof(Result)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition);

    public static TResponse Create<TResponse>(Error error)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            return (TResponse)GenericFailure.MakeGenericMethod(valueType).Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"Request handled by a Result-returning behavior must return Result or Result<T>, but returns {responseType.Name}.");
    }
}
