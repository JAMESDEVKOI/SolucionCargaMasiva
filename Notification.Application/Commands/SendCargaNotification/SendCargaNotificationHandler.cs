using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Domain.Interfaces;

namespace Notification.Application.Commands.SendCargaNotification
{
    public class SendCargaNotificationHandler : IRequestHandler<SendCargaNotificationCommand, bool>
    {
        private readonly ICargaArchivoRepository _cargaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SendCargaNotificationHandler> _logger;

        public SendCargaNotificationHandler(
            ICargaArchivoRepository cargaRepository,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork,
            ILogger<SendCargaNotificationHandler> logger)
        {
            _cargaRepository = cargaRepository;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(
            SendCargaNotificationCommand request,
            CancellationToken cancellationToken)
        {
            var notification = request.Notification;

            _logger.LogInformation(
                "Procesando notificación para carga {IdCarga}, usuario: {Usuario}, estado: {Estado}",
                notification.IdCarga, notification.Usuario, notification.Estado);

            try
            {
                // 1. Obtener carga de la BD
                var carga = await _cargaRepository.GetByIdAsync(notification.IdCarga, cancellationToken);
                if (carga == null)
                {
                    _logger.LogWarning("Carga {IdCarga} no encontrada", notification.IdCarga);
                    return false;
                }

                // 2. VALIDAR IDEMPOTENCIA - No reenviar si ya está notificado
                if (carga.Estado == CargaEstado.Notificado)
                {
                    _logger.LogInformation(
                        "Carga {IdCarga} ya está notificada. Ignorando (idempotencia).",
                        notification.IdCarga);
                    return true; // ACK el mensaje sin reenviar
                }

                // 3. Construir contenido del correo
                var subject = BuildEmailSubject(notification.Estado);
                var body = BuildEmailBody(notification, carga);

                // 4. Enviar correo
                _logger.LogInformation(
                    "Enviando correo a {Usuario} para carga {IdCarga}",
                    notification.Usuario, notification.IdCarga);

                bool emailSent = false;
                string? emailError = null;

                try
                {
                    emailSent = await _emailSender.SendEmailAsync(
                        notification.Usuario,
                        subject,
                        body,
                        isHtml: true,
                        cancellationToken);

                    if (emailSent)
                    {
                        _logger.LogInformation(
                            "Correo enviado exitosamente a {Usuario}",
                            notification.Usuario);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "No se pudo enviar correo a {Usuario}. EmailSender retornó false",
                            notification.Usuario);
                        emailError = "Email sending returned false";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al enviar correo a {Usuario}",
                        notification.Usuario);
                    emailError = ex.Message;
                    emailSent = false;
                }

                // 5. Actualizar estado en BD
                if (emailSent)
                {
                    // Email enviado OK - marcar como Notificado
                    carga.Estado = CargaEstado.Notificado;
                    carga.NotificadoAt = DateTime.UtcNow;
                    carga.EmailStatus = "Sent";
                    carga.EmailError = null;

                    if (!carga.FechaFinProceso.HasValue)
                    {
                        carga.FechaFinProceso = notification.FechaFin;
                    }

                    _logger.LogInformation(
                        "Actualizando carga {IdCarga} a estado Notificado",
                        notification.IdCarga);
                }
                else
                {
                    // Email falló - registrar error pero NO marcar como Notificado
                    carga.EmailStatus = "Failed";
                    carga.EmailError = emailError ?? "Unknown error";

                    _logger.LogWarning(
                        "Carga {IdCarga} NO marcada como Notificado debido a fallo en envío de email",
                        notification.IdCarga);
                }

                await _cargaRepository.UpdateAsync(carga, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Carga {IdCarga} actualizada. Estado: {Estado}, EmailStatus: {EmailStatus}",
                    notification.IdCarga, carga.Estado, carga.EmailStatus);

                // Retornar true solo si el email fue enviado exitosamente
                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error inesperado al procesar notificación para carga {IdCarga}",
                    notification.IdCarga);
                throw;
            }
        }

        private string BuildEmailSubject(string estado)
        {
            return estado switch
            {
                "Finalizado" => "✅ Carga Masiva Finalizada",
                "Rechazado" => "❌ Carga Masiva Rechazada",
                _ => $"Notificación de Carga Masiva - {estado}"
            };
        }

        private string BuildEmailBody(CargaFinalizadaNotificationDto notification, Domain.Entities.CargaArchivo carga)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {(notification.Estado == "Finalizado" ? "#4CAF50" : "#f44336")}; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .info-row {{ margin: 10px 0; }}
        .label {{ font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 15px; }}
        th, td {{ padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>{(notification.Estado == "Finalizado" ? "✅ Carga Finalizada" : "❌ Carga Rechazada")}</h2>
        </div>

        <div class=""content"">
            <p>Estimado/a usuario/a,</p>
            <p>Le informamos sobre el estado de su carga masiva:</p>

            <div class=""info-row"">
                <span class=""label"">ID de Carga:</span> {notification.IdCarga}
            </div>
            <div class=""info-row"">
                <span class=""label"">Archivo:</span> {carga.NombreArchivo}
            </div>
            <div class=""info-row"">
                <span class=""label"">Periodo:</span> {carga.Periodo}
            </div>
            <div class=""info-row"">
                <span class=""label"">Estado:</span> <strong>{notification.Estado}</strong>
            </div>
            <div class=""info-row"">
                <span class=""label"">Fecha:</span> {notification.FechaFin:yyyy-MM-dd HH:mm:ss}
            </div>
";

            if (notification.Estado == "Finalizado" && notification.Procesados.HasValue)
            {
                html += $@"
            <h3>Resumen del Procesamiento</h3>
            <table>
                <tr>
                    <th>Métrica</th>
                    <th>Cantidad</th>
                </tr>
                <tr>
                    <td>Registros Procesados</td>
                    <td>{notification.Procesados}</td>
                </tr>
                <tr>
                    <td>Registros Insertados</td>
                    <td style=""color: green;"">{notification.Insertados}</td>
                </tr>
                <tr>
                    <td>Registros Fallidos</td>
                    <td style=""color: red;"">{notification.Fallidos}</td>
                </tr>
            </table>
";
            }

            if (notification.Estado == "Rechazado" && !string.IsNullOrEmpty(notification.Motivo))
            {
                html += $@"
            <div class=""info-row"" style=""margin-top: 20px; padding: 15px; background-color: #ffebee; border-left: 4px solid #f44336;"">
                <span class=""label"">Motivo del Rechazo:</span><br/>
                {notification.Motivo}
            </div>
";
            }

            html += @"
        </div>

        <div class=""footer"">
            <p>Este es un correo automático. Por favor no responder.</p>
            <p>Sistema de Carga Masiva - 2025</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }
    }
}
