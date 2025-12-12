using System.Diagnostics.CodeAnalysis;


namespace Auth.Domain.Primitives
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected internal Result(bool isSuccces, Error error)
        {
            if (isSuccces && error != Error.None)
                throw new InvalidOperationException();
            if (!isSuccces && error == Error.None)
                throw new InvalidOperationException();

            IsSuccess = isSuccces;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        public static Result<TValue> Success<TValue>(TValue value)
            => new(value, true, Error.None);
        public static Result<TValue> Failure<TValue>(Error error)
            => new(default, false, error);
        public static Result<TValue> Create<TValue>(TValue? value)
            => value is not null
                ? Success(value)
                : Failure<TValue>(Error.NullValue);
    }

    public class Result<TValue> : Result
    {
        public readonly TValue? _value;
        protected internal Result(TValue? value, bool isSuccces, Error error)
            : base(isSuccces, error)
        {
            _value = value;
        }

        [NotNull]
        public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value of a failed result");

        public static implicit operator Result<TValue>(TValue value) => Create(value);
    }
}
