namespace MeetingRoomBooking.Api.Application.Common;

// Unit type for Result without payload
public readonly record struct Unit;

// Lightweight result union
public sealed record Result<T>(T? Value, ApiError? Error) where T : notnull
{
    public bool IsSuccess => Error is null;

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ApiError, TResult> onError)
    {
        if (!IsSuccess)
        {
            return onError(Error!);
        }

        if (Value is null)
        {
            throw new InvalidOperationException("Result was successful but Value was null.");
        }

        return onSuccess(Value);
    }

    public static Result<T> Success(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Success value cannot be null.");
        }

        return new(value, null);
    }

    public static Result<T> Fail(ApiError error) => new(default, error);
}
