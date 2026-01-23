namespace MeetingRoomBooking.Api.Application.Common;

// Unit type for Result without payload
public readonly record struct Unit;

// Lightweight result union
public sealed record Result<T>(T? Value, ApiError? Error)
{
    public bool IsSuccess => Error is null;

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ApiError, TResult> onError)
        => IsSuccess ? onSuccess(Value!) : onError(Error!);

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Fail(ApiError error) => new(default, error);
}
