using BulkProcessor.Application.DTOs;

namespace BulkProcessor.Application.Interfaces
{
    public interface IMessageBusService
    {
        Task PublishNotificationAsync(CargaFinalizadaNotificationDto notification, CancellationToken cancellationToken = default);
    }
}
