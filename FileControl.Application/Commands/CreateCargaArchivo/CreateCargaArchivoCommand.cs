using FileControl.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FileControl.Application.Commands.CreateCargaArchivo
{
    public record CreateCargaArchivoCommand(
        IFormFile File,
        string Periodo
    ) : IRequest<CargaArchivoResponseDto>;
}
