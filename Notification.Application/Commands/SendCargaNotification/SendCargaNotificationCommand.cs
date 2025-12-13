using MediatR;
using Notification.Application.DTOs;

namespace Notification.Application.Commands.SendCargaNotification
{
    public record SendCargaNotificationCommand(
        CargaFinalizadaNotificationDto Notification
    ) : IRequest<bool>;
}
