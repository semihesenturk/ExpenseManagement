namespace Shared.Contracts.Events;

public record ExpenseRejectedEvent
{
    public Guid ExpenseId { get; init; }
    public Guid TenantId { get; init; }
    public string RejectedByUserId { get; init; }
    public string Reason { get; init; }
}