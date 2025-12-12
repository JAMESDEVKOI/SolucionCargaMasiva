using Auth.Application.Abstractions.Interfaces.Caching;
using Auth.Application.Abstractions.Interfaces.Sessions;

namespace Auth.Infrastructure.Sessions
{
    internal sealed class SessionManager : ISessionManager
    {
        private readonly ICacheService _cacheService;
        private const string SessionKeyPrefix = "session:";
        private const string UserSessionsKeyPrefix = "user-sessions:";
        private static readonly TimeSpan SessionExpiry = TimeSpan.FromDays(30);

        public SessionManager(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<string> CreateSessionAsync(Guid userId, string ipAddress, string userAgent)
        {
            var sessionId = Guid.NewGuid().ToString();
            var sessionInfo = new SessionInfo
            {
                SessionId = sessionId,
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsActive = true
            };

            var sessionKey = GetSessionKey(sessionId);
            await _cacheService.SetAsync(sessionKey, sessionInfo, SessionExpiry);

            var userSessionsKey = GetUserSessionsKey(userId);
            var userSessions = await _cacheService.GetAsync<HashSet<string>>(userSessionsKey) ?? new HashSet<string>();
            userSessions.Add(sessionId);
            await _cacheService.SetAsync(userSessionsKey, userSessions, SessionExpiry);

            return sessionId;
        }

        public async Task<bool> ValidateSessionAsync(string sessionId)
        {
            var sessionKey = GetSessionKey(sessionId);
            var exists = await _cacheService.ExistsAsync(sessionKey);
            if (!exists)
                return false;

            var sessionInfo = await _cacheService.GetAsync<SessionInfo>(sessionKey);
            return sessionInfo?.IsActive ?? false;
        }

        public async Task UpdateSessionActivityAsync(string sessionId)
        {
            var sessionKey = GetSessionKey(sessionId);
            var sessionInfo = await _cacheService.GetAsync<SessionInfo>(sessionKey);

            if (sessionInfo is not null)
            {
                sessionInfo.LastActivityAt = DateTime.UtcNow;
                await _cacheService.SetAsync(sessionKey, sessionInfo, SessionExpiry);
            }
        }

        public async Task RevokeSessionAsync(string sessionId)
        {
            var sessionKey = GetSessionKey(sessionId);
            var sessionInfo = await _cacheService.GetAsync<SessionInfo>(sessionKey);

            if (sessionInfo is not null)
            {
                sessionInfo.IsActive = false;
                await _cacheService.SetAsync(sessionKey, sessionInfo, TimeSpan.FromHours(1));

                var userSessionsKey = GetUserSessionsKey(sessionInfo.UserId);
                var userSessions = await _cacheService.GetAsync<HashSet<string>>(userSessionsKey);
                if (userSessions is not null)
                {
                    userSessions.Remove(sessionId);
                    await _cacheService.SetAsync(userSessionsKey, userSessions, SessionExpiry);
                }
            }
        }

        public async Task RevokeAllUserSessionsAsync(Guid userId)
        {
            var userSessionsKey = GetUserSessionsKey(userId);
            var userSessions = await _cacheService.GetAsync<HashSet<string>>(userSessionsKey);

            if (userSessions is not null)
            {
                foreach (var sessionId in userSessions)
                {
                    await RevokeSessionAsync(sessionId);
                }
            }

            await _cacheService.RemoveAsync(userSessionsKey);
        }

        public async Task<IEnumerable<SessionInfo>> GetUserActiveSessionsAsync(Guid userId)
        {
            var userSessionsKey = GetUserSessionsKey(userId);
            var userSessions = await _cacheService.GetAsync<HashSet<string>>(userSessionsKey);

            if (userSessions is null || !userSessions.Any())
                return Enumerable.Empty<SessionInfo>();

            var activeSessions = new List<SessionInfo>();
            foreach (var sessionId in userSessions)
            {
                var sessionInfo = await GetSessionInfoAsync(sessionId);
                if (sessionInfo?.IsActive == true)
                {
                    activeSessions.Add(sessionInfo);
                }
            }

            return activeSessions;
        }

        public async Task<SessionInfo?> GetSessionInfoAsync(string sessionId)
        {
            var sessionKey = GetSessionKey(sessionId);
            return await _cacheService.GetAsync<SessionInfo>(sessionKey);
        }

        public async Task<int> CountUserActiveSessionsAsync(Guid userId)
        {
            var activeSessions = await GetUserActiveSessionsAsync(userId);
            return activeSessions.Count();
        }

        private static string GetSessionKey(string sessionId) => $"{SessionKeyPrefix}{sessionId}";
        private static string GetUserSessionsKey(Guid userId) => $"{UserSessionsKeyPrefix}{userId}";
    }
}
