using MediatR;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public record CreateExpenseRequestCommand(decimal Amount, string Description) : IRequest<CreateExpenseRequestDto>;