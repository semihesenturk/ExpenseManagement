using MediatR;

namespace Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;

public record ApproveExpenseRequestCommand(Guid ExpenseRequestId, string? Note = null) 
    : IRequest<Unit>;