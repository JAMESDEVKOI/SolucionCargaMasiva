using Auth.Application.Abstractions.Interfaces.Repositories;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Primitives;

namespace Auth.Application.UseCases.Users.Queries.GetAllUsers
{
    internal sealed class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, UserListResponse>
    {
        private readonly IUserQueryRepository _userQueryRepository;

        public GetAllUsersQueryHandler(IUserQueryRepository userQueryRepository)
        {
            _userQueryRepository = userQueryRepository;
        }

        public async Task<Result<UserListResponse>> Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken)
        {
            var users = await _userQueryRepository.GetAllAsync(
                request.Page,
                request.PageSize,
                cancellationToken);

            var totalCount = await _userQueryRepository.GetTotalCountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var response = new UserListResponse(
                users,
                totalCount,
                request.Page,
                request.PageSize,
                totalPages);

            return Result.Success(response);
        }
    }
}
