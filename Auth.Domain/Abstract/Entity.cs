namespace Auth.Domain.Abstract
{
    public abstract class Entity<TValue>
    {
        protected Entity() { }
        protected Entity(TValue id)
        {
            Id = id;
        }
        public TValue Id { get; init; } = default!;
    }
}
