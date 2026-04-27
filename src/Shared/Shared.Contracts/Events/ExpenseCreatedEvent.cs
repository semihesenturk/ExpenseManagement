namespace Shared.Contracts.Events;

public record ExpenseCreatedEvent
{
    public Guid ExpenseId { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
}