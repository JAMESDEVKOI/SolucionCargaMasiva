using Auth.Application.Abstractions.Interfaces.Common;
using Auth.Application.Exceptions;
using Auth.Domain.Interface;
using Auth.Domain.UserRole;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Auth.Infrastructure.Persistence
{
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPublisher _publisher;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IDateTimeProvider dateTimeProvider,
            IPublisher publisher
        ) : base(options)
        {
            _dateTimeProvider = dateTimeProvider;
            _publisher = publisher;
        }

        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                UpdateAuditableEntities();

                await PublishDomainEventsAsync(cancellationToken);

                var result = await base.SaveChangesAsync(cancellationToken);

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("La excepcion por concurrencia se disparo", ex);
            }
        }

        private void UpdateAuditableEntities()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = _dateTimeProvider.currentTime;
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = _dateTimeProvider.currentTime;
                }
            }
        }

        private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
        {
            var domainEvents = ChangeTracker
                .Entries<IHasDomainEvent>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    var events = entity.GetDomainEvents();
                    entity.ClearDomainEvents();
                    return events;
                })
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
