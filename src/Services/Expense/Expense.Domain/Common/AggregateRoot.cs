namespace Expense.Domain.Common;

public abstract class AggregateRoot : BaseEntity
{
    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(object @event)
    {
        _domainEvents.Add(@event);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}