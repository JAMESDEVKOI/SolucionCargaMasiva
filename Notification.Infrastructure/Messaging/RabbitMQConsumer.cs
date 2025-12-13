using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Commands.SendCargaNotification;
using Notification.Application.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Notification.Infrastructure.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQConsumer(
            IOptions<RabbitMQSettings> settings,
            ILogger<RabbitMQConsumer> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _settings = settings.Value;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification RabbitMQ Consumer iniciando...");

            await Task.Delay(5000, stoppingToken); // Esperar a que RabbitMQ esté listo

            InitializeRabbitMQ();

            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Notification RabbitMQ Consumer deteniendo...");
                _channel?.Close();
                _connection?.Close();
            });

            await Task.CompletedTask;
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Configurar QoS - procesar 1 mensaje a la vez
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            // Declarar exchange y queue
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

            // Configurar consumidor asíncrono
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceivedAsync;

            _channel.BasicConsume(
                queue: _settings.NotificacionesQueue,
                autoAck: false, // ACK manual
                consumer: consumer);

            _logger.LogInformation(
                "Notification RabbitMQ Consumer escuchando en cola: {Queue}",
                _settings.NotificacionesQueue);
        }

        private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageId = eventArgs.BasicProperties?.MessageId ?? "unknown";

            _logger.LogInformation(
                "Mensaje de notificación recibido. MessageId: {MessageId}, DeliveryTag: {DeliveryTag}",
                messageId, eventArgs.DeliveryTag);

            try
            {
                var body = eventArgs.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                _logger.LogDebug("Contenido del mensaje: {Message}", messageJson);

                // Deserializar mensaje
                var notification = JsonSerializer.Deserialize<CargaFinalizadaNotificationDto>(
                    messageJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (notification == null)
                {
                    _logger.LogWarning("Mensaje nulo o inválido. NACK sin requeue");
                    _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                // Procesar notificación usando MediatR
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new SendCargaNotificationCommand(notification);
                var result = await mediator.Send(command);

                if (result)
                {
                    // ACK el mensaje - notificación enviada exitosamente
                    _channel?.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    _logger.LogInformation(
                        "Notificación procesada exitosamente. MessageId: {MessageId}, IdCarga: {IdCarga}",
                        messageId, notification.IdCarga);
                }
                else
                {
                    // Email falló - NACK con requeue para reintentar
                    _logger.LogWarning(
                        "Fallo al enviar email. Reencolando mensaje. MessageId: {MessageId}, IdCarga: {IdCarga}",
                        messageId, notification.IdCarga);
                    _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Error al deserializar mensaje. MessageId: {MessageId}. NACK sin requeue",
                    messageId);
                _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error inesperado al procesar notificación. MessageId: {MessageId}. NACK con requeue",
                    messageId);

                // NACK con requeue para reintentar
                _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
