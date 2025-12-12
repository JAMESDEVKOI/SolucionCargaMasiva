using Auth.Application.Abstractions.Interfaces.Identity;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Primitives;
using Auth.Domain.User;
using Auth.Domain.User.ValueObject;

namespace Auth.Application.UseCases.Auth.Commands.RefreshToken
{
    internal sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
        }

        public async Task<Result<RefreshTokenResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var isValid = await _jwtProvider.ValidateRefreshTokenAsync(
                request.UserId,
                request.RefreshToken,
                cancellationToken);

            if (!isValid)
            {
                return Result.Failure<RefreshTokenResponse>(
                    new Error("Auth.InvalidRefreshToken", "Refresh token inválido o expirado"));
            }

            if (!Guid.TryParse(request.UserId, out var userId))
            {
                return Result.Failure<RefreshTokenResponse>(
                    new Error("Auth.InvalidUserId", "UserId inválido"));
            }

            var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken);

            if (user is null)
            {
                return Result.Failure<RefreshTokenResponse>(
                    new Error("Auth.UserNotFound", "Usuario no encontrado"));
            }

            var newAccessToken = await _jwtProvider.GenerateAccessTokenAsync(user, cancellationToken);
            var newRefreshToken = _jwtProvider.GenerateRefreshToken();

            await _jwtProvider.StoreRefreshTokenAsync(
                request.UserId,
                newRefreshToken,
                cancellationToken);

            var response = new RefreshTokenResponse(newAccessToken, newRefreshToken);

            return Result.Success(response);
        }
    }
}
