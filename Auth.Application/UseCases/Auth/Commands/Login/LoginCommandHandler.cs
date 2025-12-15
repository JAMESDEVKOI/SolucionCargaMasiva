using Auth.Application.Abstractions.Interfaces.Identity;
using Auth.Application.Abstractions.Interfaces.Sessions;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Primitives;
using Auth.Domain.User;
using Auth.Domain.User.ValueObject;

namespace Auth.Application.UseCases.Auth.Commands.Login
{
    internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly ISessionManager _sessionManager;
        private readonly ICookieAuthenticationProvider _cookieProvider;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            ISessionManager sessionManager,
            ICookieAuthenticationProvider cookieProvider)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _sessionManager = sessionManager;
            _cookieProvider = cookieProvider;
        }

        public async Task<Result<LoginResponse>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var email = new Email(request.Email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is null)
            {
                return Result.Failure<LoginResponse>(
                    new Error("Auth.InvalidCredentials", "Credenciales inválidas"));
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password.Value);

            if (!isPasswordValid)
            {
                return Result.Failure<LoginResponse>(
                    new Error("Auth.InvalidCredentials", "Credenciales inválidas"));
            }

            var accessToken = await _jwtProvider.GenerateAccessTokenAsync(user, cancellationToken);
            var refreshToken = _jwtProvider.GenerateRefreshToken();
            var sessionId = await _sessionManager.CreateSessionAsync(
                user.Id!.Value,
                request.IpAddress ?? "unknown",
                request.UserAgent ?? "unknown");

            await _jwtProvider.StoreRefreshTokenAsync(
                user.Id!.Value.ToString(),
                refreshToken,
                cancellationToken);

            // Establecer cookies con los tokens
            _cookieProvider.SetAuthenticationCookies(accessToken, refreshToken);

            var response = new LoginResponse(
                accessToken,
                refreshToken,
                sessionId,
                user.Id!.Value,
                user.Email.Value,
                $"{user.Name.Value} {user.LastName.Value}"
            );

            return Result.Success(response);
        }
    }
}
