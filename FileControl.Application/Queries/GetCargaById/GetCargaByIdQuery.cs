using FileControl.Application.DTOs;
using MediatR;

namespace FileControl.Application.Queries.GetCargaById
{
    public record GetCargaByIdQuery(int Id) : IRequest<CargaArchivoListDto?>;
}
