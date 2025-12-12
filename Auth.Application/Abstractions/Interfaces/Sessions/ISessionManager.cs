namespace Auth.Application.Abstractions.Interfaces.Sessions
{
    public interface ISessionManager
    {
        Task<string> CreateSessionAsync(Guid userId, string ipAddress, string userAgent);
        Task<bool> ValidateSessionAsync(string sessionId);
        Task UpdateSessionActivityAsync(string sessionId);
        Task RevokeSessionAsync(string sessionId);
        Task RevokeAllUserSessionsAsync(Guid userId);
        Task<IEnumerable<SessionInfo>> GetUserActiveSessionsAsync(Guid userId);
        Task<SessionInfo?> GetSessionInfoAsync(string sessionId);
        Task<int> CountUserActiveSessionsAsync(Guid userId);
    }
}
