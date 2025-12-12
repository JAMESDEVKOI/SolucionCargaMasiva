using Auth.Domain.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Auth.Infrastructure.Persistence.Interceptors
{
    internal sealed class PublishDomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;

        public PublishDomainEventsInterceptor(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                await PublishDomainEventsAsync(eventData.Context, cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private async Task PublishDomainEventsAsync(
            DbContext context,
            CancellationToken cancellationToken)
        {
            var domainEvents = context.ChangeTracker
                .Entries<IHasDomainEvent>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    var events = entity.GetDomainEvents().ToList();
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
