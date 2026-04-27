using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using FluentValidation;

public class CreateExpenseRequestCommandValidator : AbstractValidator<CreateExpenseRequestCommand>
{
    public CreateExpenseRequestCommandValidator()
    {
       RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(250);
    }
}