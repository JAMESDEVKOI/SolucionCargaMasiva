using Auth.Application.Abstractions.Interfaces.Identity;
using Auth.Application.Abstractions.Interfaces.Sessions;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Primitives;

namespace Auth.Application.UseCases.Auth.Commands.Logout
{
    internal sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly ISessionManager _sessionManager;
        private readonly IJwtProvider _jwtProvider;
        private readonly ICookieAuthenticationProvider _cookieProvider;

        public LogoutCommandHandler(
            ISessionManager sessionManager,
            IJwtProvider jwtProvider,
            ICookieAuthenticationProvider cookieProvider)
        {
            _sessionManager = sessionManager;
            _jwtProvider = jwtProvider;
            _cookieProvider = cookieProvider;
        }

        public async Task<Result> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            await _sessionManager.RevokeSessionAsync(request.SessionId);

            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                await _jwtProvider.RevokeTokenAsync(request.AccessToken, cancellationToken);
            }

            // Eliminar cookies de autenticación
            _cookieProvider.RemoveAuthenticationCookies();

            return Result.Success();
        }
    }
}
