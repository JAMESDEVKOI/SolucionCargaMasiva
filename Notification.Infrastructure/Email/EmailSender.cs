using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Notification.Application.Interfaces;
using Polly;
using Polly.Retry;

namespace Notification.Infrastructure.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailSender> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public EmailSender(
            IOptions<SmtpSettings> settings,
            ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            // Configurar política de reintentos con Polly
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Reintento {RetryCount} después de {TimeSpan}s debido a: {Message}",
                            retryCount, timeSpan.TotalSeconds, exception.Message);
                    });
        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await SendEmailInternalAsync(to, subject, body, isHtml, cancellationToken);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al enviar correo a {To} después de todos los reintentos",
                    to);
                return false;
            }
        }

        private async Task<bool> SendEmailInternalAsync(
            string to,
            string subject,
            string body,
            bool isHtml,
            CancellationToken cancellationToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sistema de Carga Masiva", _settings.From));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                _logger.LogDebug(
                    "Conectando a SMTP {Host}:{Port}",
                    _settings.Host, _settings.Port);

                // Configurar opciones de seguridad
                var secureSocketOptions = _settings.UseStartTLS
                    ? SecureSocketOptions.StartTls
                    : (_settings.UseSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);

                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    secureSocketOptions,
                    cancellationToken);

                if (!string.IsNullOrEmpty(_settings.User))
                {
                    await client.AuthenticateAsync(
                        _settings.User,
                        _settings.Password,
                        cancellationToken);
                }

                _logger.LogDebug("Enviando correo a {To}", to);

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation(
                    "Correo enviado exitosamente a {To}",
                    to);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al enviar correo a {To}: {Message}",
                    to, ex.Message);
                throw;
            }
        }
    }
}
