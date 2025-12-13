using BulkProcessor.Application.DTOs;
using BulkProcessor.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BulkProcessor.Infrastructure.Messaging
{
    public class RabbitMQPublisher : IMessageBusService, IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQPublisher> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQPublisher(
            IOptions<RabbitMQSettings> settings,
            ILogger<RabbitMQPublisher> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _settings.NotificacionesExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.NotificacionesQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: _settings.NotificacionesQueue,
                exchange: _settings.NotificacionesExchange,
                routingKey: _settings.NotificacionesQueue);

            _logger.LogInformation(
                "RabbitMQ Publisher inicializado. Queue: {Queue}",
                _settings.NotificacionesQueue);
        }

        public Task PublishNotificationAsync(
            CargaFinalizadaNotificationDto notification,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var message = JsonSerializer.Serialize(notification);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: _settings.NotificacionesExchange,
                    routingKey: _settings.NotificacionesQueue,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Notificación publicada. IdCarga: {IdCarga}, Estado: {Estado}",
                    notification.IdCarga, notification.Estado);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al publicar notificación. IdCarga: {IdCarga}",
                    notification.IdCarga);
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
