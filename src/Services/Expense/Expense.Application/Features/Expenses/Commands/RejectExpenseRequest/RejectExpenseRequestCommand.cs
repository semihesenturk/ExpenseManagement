using MediatR;

namespace Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;

public record RejectExpenseRequestCommand(Guid ExpenseRequestId, string? Note) : IRequest<Unit>;