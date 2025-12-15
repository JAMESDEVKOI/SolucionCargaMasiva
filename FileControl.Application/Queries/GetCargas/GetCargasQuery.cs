using FileControl.Application.DTOs;
using MediatR;

namespace FileControl.Application.Queries.GetCargas
{
    public record GetCargasQuery(string? Usuario = null) : IRequest<List<CargaArchivoListDto>>;
}
