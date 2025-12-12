namespace Auth.Domain.Interface
{
    public interface IHasDomainEvent
    {
        IReadOnlyList<IDomainEvent> GetDomainEvents();
        void ClearDomainEvents();
    }
}
