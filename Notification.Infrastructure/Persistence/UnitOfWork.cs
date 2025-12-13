using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NotificationDbContext _context;

        public UnitOfWork(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
