using MediatR;

public record ApproveExpenseRequestCommand(Guid ExpenseRequestId, Guid ApproverId, string? Note = null)
    : IRequest<Unit>;