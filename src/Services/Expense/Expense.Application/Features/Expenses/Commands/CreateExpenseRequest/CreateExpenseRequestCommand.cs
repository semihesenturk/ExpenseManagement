using MediatR;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestCommand : IRequest<CreateExpenseRequestDto>
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = default!;
}