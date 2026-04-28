namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestDto
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
}