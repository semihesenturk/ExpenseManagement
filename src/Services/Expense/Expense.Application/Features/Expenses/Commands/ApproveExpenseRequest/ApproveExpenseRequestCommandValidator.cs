using FluentValidation;

namespace Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;

public class ApproveExpenseRequestCommandValidator : AbstractValidator<ApproveExpenseRequestCommand>
{
    public ApproveExpenseRequestCommandValidator()
    {
        RuleFor(x => x.ExpenseRequestId).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
    }
}