using Auth.Domain.Interface;

namespace Auth.Domain.Abstract
{
    public abstract class AggregateRoot<TEntityId> : Entity<TEntityId>, IHasDomainEvent
    {

        protected AggregateRoot() : base() { }
        protected AggregateRoot(TEntityId id) : base(id) { }
        public List<IDomainEvent> _domainEvents = new();


        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.ToList();
        }


        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void RaiseDomainEvent(IDomainEvent domainEvent) 
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
