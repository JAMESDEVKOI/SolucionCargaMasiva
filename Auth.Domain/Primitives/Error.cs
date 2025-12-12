namespace Auth.Domain.Primitives
{
    public record Error(string Code, string Message)
    {

        public static Error None = new(string.Empty, string.Empty);
        public static Error NullValue = new("Error.NullValue", "Un valor Null fue ingresado");

    }
}
