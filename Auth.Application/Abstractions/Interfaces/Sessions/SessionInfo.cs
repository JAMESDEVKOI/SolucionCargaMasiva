namespace Auth.Application.Abstractions.Interfaces.Sessions
{
    public class SessionInfo
    {
        public string SessionId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public bool IsActive { get; set; }
    }
}
