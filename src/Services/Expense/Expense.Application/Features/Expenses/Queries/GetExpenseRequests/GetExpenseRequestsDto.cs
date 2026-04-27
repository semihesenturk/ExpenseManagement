namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequests;

public class GetExpenseRequestsDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = default!;
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = default!;
}