using Auth.Domain.User;
using Auth.Domain.User.ValueObject;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence.Repositories
{
    internal sealed class UserRepository
        : Repository<User, UserId>, IUserRepository
    {
        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<User>()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        public async Task<bool> IsUserExists(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<User>()
            .AnyAsync(x => x.Email == email);
        }

        public override void Add(User user)
        {
            DbContext.Add(user);
        }
    }
}
