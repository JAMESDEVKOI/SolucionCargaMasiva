using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Interface;
using Auth.Domain.Primitives;
using Auth.Domain.User;
using Auth.Domain.User.Errors;
using Auth.Domain.User.ValueObject;

namespace Auth.Application.UseCases.Users.Commands.CreateUser
{
    internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            var email = new Email(request.Email);
            var userExists = await _userRepository.IsUserExists(email, cancellationToken);

            if (userExists)
            {
                return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : new Phone(request.Phone);

            var user = User.Create(
                new Name(request.Name),
                new LastName(request.LastName),
                email,
                new Password(passwordHash),
                phone
            );

            _userRepository.Add(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return user.Id!.Value;
        }
    }
}
