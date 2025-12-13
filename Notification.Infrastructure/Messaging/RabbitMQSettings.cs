namespace Notification.Infrastructure.Messaging
{
    public class RabbitMQSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string NotificacionesQueue { get; set; } = "notificaciones";
        public string NotificacionesExchange { get; set; } = "notificaciones_exchange";
    }
}
