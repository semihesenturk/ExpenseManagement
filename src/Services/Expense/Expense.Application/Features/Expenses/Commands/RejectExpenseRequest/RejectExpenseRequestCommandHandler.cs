using MediatR;
using Expense.Application.Contracts.Persistence;

namespace Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;

public class RejectExpenseRequestCommandHandler 
    : IRequestHandler<RejectExpenseRequestCommand, Unit>
{
    private readonly IExpenseRequestRepository _repository;

    public RejectExpenseRequestCommandHandler(IExpenseRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(RejectExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var expense = await _repository.GetByIdAsync(request.ExpenseRequestId, cancellationToken)
                      ?? throw new KeyNotFoundException("Expense request not found.");

        expense.Reject(request.ApproverId, request.Note);
        await _repository.UpdateAsync(expense, cancellationToken);

        return Unit.Value;
    }
}