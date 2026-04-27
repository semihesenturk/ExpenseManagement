namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestDto
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime RequestDate { get; set; }
}