using Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;
using FluentValidation;

public class RejectExpenseRequestCommandValidator : AbstractValidator<RejectExpenseRequestCommand>
{
    public RejectExpenseRequestCommandValidator()
    {
        RuleFor(x => x.ExpenseRequestId)
            .NotEmpty().WithMessage("Harcama ID boş olamaz.");
        
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Red nedeni zorunludur.")
            .MinimumLength(10).WithMessage("Red nedeni en az 10 karakter olmalıdır.");
    }
}