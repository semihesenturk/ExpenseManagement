using Expense.Domain.Enums;
using MediatR;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestCommand : IRequest<CreateExpenseRequestDto>
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public Currency Currency { get; set; }
}