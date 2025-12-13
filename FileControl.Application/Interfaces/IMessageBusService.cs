using FileControl.Application.DTOs;

namespace FileControl.Application.Interfaces
{
    public interface IMessageBusService
    {
        Task PublishCargaMasivaAsync(
            CargaMasivaMessageDto message,
            CancellationToken cancellationToken = default);
    }
}
