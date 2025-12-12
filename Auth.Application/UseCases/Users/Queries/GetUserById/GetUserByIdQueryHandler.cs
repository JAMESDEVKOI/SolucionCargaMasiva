using Auth.Application.Abstractions.Interfaces.Repositories;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Primitives;

namespace Auth.Application.UseCases.Users.Queries.GetUserById
{
    internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
    {
        private readonly IUserQueryRepository _userQueryRepository;

        public GetUserByIdQueryHandler(IUserQueryRepository userQueryRepository)
        {
            _userQueryRepository = userQueryRepository;
        }

        public async Task<Result<UserResponse>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                return Result.Failure<UserResponse>(
                    new Error("User.NotFound", $"User with ID {request.UserId} was not found"));
            }

            return Result.Success(new UserResponse(user));
        }
    }
}
