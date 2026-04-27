using MediatR;

namespace Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;

public record RejectExpenseRequestCommand(Guid ExpenseRequestId, Guid ApproverId, string? Note = null)
    : IRequest<Unit>;