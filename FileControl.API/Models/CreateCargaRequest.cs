namespace FileControl.API.Models
{
    public class CreateCargaRequest
    {
        public IFormFile File { get; set; } = null!;
        public string Periodo { get; set; } = string.Empty;
    }
}
