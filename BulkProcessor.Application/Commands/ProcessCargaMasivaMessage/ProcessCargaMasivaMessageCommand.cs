using BulkProcessor.Application.DTOs;
using MediatR;

namespace BulkProcessor.Application.Commands.ProcessCargaMasivaMessage
{
    public record ProcessCargaMasivaMessageCommand(
        CargaMasivaRequestedDto Message
    ) : IRequest<bool>;
}
