namespace Auth.Application.Exceptions
{
    public sealed class ValidationException : Exception
    {
        public ValidationException(IEnumerable<ValidationError> errors)
        {
            Errors = errors;
        }

        public IEnumerable<ValidationError> Errors { get; }

        public override string Message =>
            $"Validation failed: {string.Join(", ", Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))}";
    }
}
