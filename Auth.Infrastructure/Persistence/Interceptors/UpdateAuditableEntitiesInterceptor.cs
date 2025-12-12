using Auth.Application.Abstractions.Interfaces.Common;
using Auth.Domain.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Auth.Infrastructure.Persistence.Interceptors
{
    internal sealed class UpdateAuditableEntitiesInterceptor : SaveChangesInterceptor
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateAuditableEntitiesInterceptor(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                UpdateAuditableEntities(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            var utcNow = _dateTimeProvider.currentTime;

            var entries = context.ChangeTracker
                .Entries<IAuditableEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    SetCurrentPropertyValue(entry, nameof(IAuditableEntity.CreatedAt), utcNow);
                }

                if (entry.State == EntityState.Modified)
                {
                    SetCurrentPropertyValue(entry, nameof(IAuditableEntity.UpdatedAt), utcNow);
                }
            }
        }

        private static void SetCurrentPropertyValue(
            EntityEntry entry,
            string propertyName,
            DateTime utcNow)
        {
            entry.Property(propertyName).CurrentValue = utcNow;
        }
    }
}
