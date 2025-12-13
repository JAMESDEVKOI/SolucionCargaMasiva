namespace FileControl.Infrastructure.Messaging
{
    public class RabbitMQSettings
    {
        public const string SectionName = "RabbitMQ";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string QueueName { get; set; } = "carga_masiva";
    }
}
