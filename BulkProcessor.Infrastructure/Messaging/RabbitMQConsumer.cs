using BulkProcessor.Application.Commands.ProcessCargaMasivaMessage;
using BulkProcessor.Application.DTOs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BulkProcessor.Infrastructure.Messaging
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
            _logger.LogInformation("RabbitMQ Consumer iniciando...");

            await Task.Delay(5000, stoppingToken);

            InitializeRabbitMQ();

            stoppingToken.Register(() =>
            {
                _logger.LogInformation("RabbitMQ Consumer deteniendo...");
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

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _channel.ExchangeDeclare(
                exchange: _settings.CargaMasivaExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.CargaMasivaQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: _settings.CargaMasivaQueue,
                exchange: _settings.CargaMasivaExchange,
                routingKey: _settings.CargaMasivaQueue);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceivedAsync;

            _channel.BasicConsume(
                queue: _settings.CargaMasivaQueue,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation(
                "RabbitMQ Consumer escuchando en cola: {Queue}",
                _settings.CargaMasivaQueue);
        }

        private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageId = eventArgs.BasicProperties?.MessageId ?? "unknown";

            _logger.LogInformation(
                "Mensaje recibido. MessageId: {MessageId}, DeliveryTag: {DeliveryTag}",
                messageId, eventArgs.DeliveryTag);

            try
            {
                var body = eventArgs.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                _logger.LogDebug("Contenido del mensaje: {Message}", messageJson);
                var message = JsonSerializer.Deserialize<CargaMasivaRequestedDto>(messageJson);

                if (message == null)
                {
                    _logger.LogWarning("Mensaje nulo o inválido. NACK sin requeue");
                    _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new ProcessCargaMasivaMessageCommand(message);
                var result = await mediator.Send(command);

                if (result)
                {
                    _channel?.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    _logger.LogInformation(
                        "Mensaje procesado exitosamente. MessageId: {MessageId}, IdCarga: {IdCarga}",
                        messageId, message.IdCarga);
                }
                else
                {                    
                    _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    _logger.LogWarning(
                        "Mensaje procesado pero con resultado negativo. MessageId: {MessageId}",
                        messageId);
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
                    "Error inesperado al procesar mensaje. MessageId: {MessageId}. NACK con requeue",
                    messageId);                
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
