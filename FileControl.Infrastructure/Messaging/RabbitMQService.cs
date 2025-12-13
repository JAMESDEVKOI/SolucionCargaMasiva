using FileControl.Application.DTOs;
using FileControl.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace FileControl.Infrastructure.Messaging
{
    public class RabbitMQService : IMessageBusService, IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly IConnection? _connection;
        private readonly IModel? _channel;

        public RabbitMQService(
            IOptions<RabbitMQSettings> settings,
            ILogger<RabbitMQService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.Host,
                    Port = _settings.Port,
                    UserName = _settings.Username,
                    Password = _settings.Password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar la cola si no existe
                _channel.QueueDeclare(
                    queue: _settings.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation(
                    "Conexión a RabbitMQ establecida. Queue: {QueueName}",
                    _settings.QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con RabbitMQ");
                throw;
            }
        }

        public async Task PublishCargaMasivaAsync(
            CargaMasivaMessageDto message,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_channel == null)
                {
                    throw new InvalidOperationException("El canal de RabbitMQ no está inicializado");
                }

                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: _settings.QueueName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Mensaje publicado en RabbitMQ. IdCarga: {IdCarga}, Queue: {QueueName}",
                    message.IdCarga, _settings.QueueName);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al publicar mensaje en RabbitMQ. IdCarga: {IdCarga}",
                    message.IdCarga);
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
