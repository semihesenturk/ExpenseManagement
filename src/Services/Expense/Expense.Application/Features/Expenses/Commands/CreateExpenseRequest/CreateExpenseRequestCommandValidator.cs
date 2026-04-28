using FluentValidation;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestCommandValidator : AbstractValidator<CreateExpenseRequestCommand>
{
    public CreateExpenseRequestCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar sıfırdan büyük olmalıdır.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz.")
            .MinimumLength(20).WithMessage("Açıklama en az 20 karakter olmalıdır.")  // ✅ eklendi
            .MaximumLength(250);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Geçerli bir kategori seçiniz.");  // ✅ eklendi

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Geçerli bir para birimi seçiniz.");  // ✅ eklendi
    }
}