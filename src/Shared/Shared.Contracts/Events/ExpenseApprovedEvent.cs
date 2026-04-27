namespace Shared.Contracts.Events;

public record ExpenseApprovedEvent
{
    public Guid ExpenseId { get; init; }
    public Guid TenantId { get; init; }
    public string ApprovedByUserId { get; init; }
}