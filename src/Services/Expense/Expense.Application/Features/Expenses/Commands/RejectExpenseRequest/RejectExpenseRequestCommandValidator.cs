using FluentValidation;

namespace Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;

public class RejectExpenseRequestCommandValidator : AbstractValidator<RejectExpenseRequestCommand>
{
    public RejectExpenseRequestCommandValidator()
    {
        RuleFor(x => x.ExpenseRequestId).NotEmpty();
    }
}