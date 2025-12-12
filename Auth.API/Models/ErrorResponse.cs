namespace Auth.API.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public ErrorDetail[] Errors { get; set; } = Array.Empty<ErrorDetail>();
    }

    public class ErrorDetail
    {
        public string Field { get; set; } = string.Empty;
        public string[] Messages { get; set; } = Array.Empty<string>();
    }
}
