using MediatR;
using Expense.Application.Contracts.Persistence;

namespace Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;

public class ApproveExpenseRequestCommandHandler 
    : IRequestHandler<ApproveExpenseRequestCommand, Unit>
{
    private readonly IExpenseRequestRepository _repository;

    public ApproveExpenseRequestCommandHandler(IExpenseRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(ApproveExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var expense = await _repository.GetByIdAsync(request.ExpenseRequestId)
                      ?? throw new KeyNotFoundException("Expense request not found.");

        expense.Approve(request.ApproverId, request.Note);

        await _repository.UpdateAsync(expense);

        return Unit.Value;
    }
}