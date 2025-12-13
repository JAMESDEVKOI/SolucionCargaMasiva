using FluentValidation;

namespace FileControl.Application.Commands.CreateCargaArchivo
{
    public class CreateCargaArchivoCommandValidator : AbstractValidator<CreateCargaArchivoCommand>
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExtensions = { ".xlsx" };

        public CreateCargaArchivoCommandValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("El archivo es requerido");

            RuleFor(x => x.File.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage($"El archivo no debe superar {MaxFileSize / 1024 / 1024} MB")
                .When(x => x.File != null);

            RuleFor(x => x.File.FileName)
                .Must(fileName => AllowedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Solo se permiten archivos .xlsx")
                .When(x => x.File != null);

            RuleFor(x => x.Periodo)
                .NotEmpty()
                .WithMessage("El periodo es requerido")
                .Matches(@"^\d{4}-\d{2}$")
                .WithMessage("El formato del periodo debe ser YYYY-MM (ej: 2025-02)");
        }
    }
}
