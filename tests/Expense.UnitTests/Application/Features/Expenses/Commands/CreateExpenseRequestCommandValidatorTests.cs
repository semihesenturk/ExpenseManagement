using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Domain.Enums;
using FluentValidation.TestHelper;

namespace Expense.UnitTests.Application.Features.Expenses.Commands;

public class CreateExpenseRequestCommandValidatorTests
{
    private readonly CreateExpenseRequestCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenAmountIsZeroOrLess_ShouldHaveValidationError()
    {
        var command = new CreateExpenseRequestCommand
        {
            Amount = 0,
            Description = "Yirmi karakterden uzun açıklama",
            Category = ExpenseCategory.Travel,
            Currency = Currency.TRY
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Tutar sıfırdan büyük olmalıdır.");
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_ShouldHaveValidationError()
    {
        var command = new CreateExpenseRequestCommand
        {
            Amount = 1500,
            Description = string.Empty,
            Category = ExpenseCategory.Travel,
            Currency = Currency.TRY
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionIsShorterThan20Chars_ShouldHaveValidationError()
    {
        var command = new CreateExpenseRequestCommand
        {
            Amount = 1500,
            Description = "Kısa açıklama",
            Category = ExpenseCategory.Travel,
            Currency = Currency.TRY
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Açıklama en az 20 karakter olmalıdır.");
    }

    [Fact]
    public void Validate_WhenCategoryIsInvalid_ShouldHaveValidationError()
    {
        var command = new CreateExpenseRequestCommand
        {
            Amount = 1500,
            Description = "Yirmi karakterden uzun açıklama",
            Category = (ExpenseCategory)99,
            Currency = Currency.TRY
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Geçerli bir kategori seçiniz.");
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        var command = new CreateExpenseRequestCommand
        {
            Amount = 1500,
            Description = "Yirmi karakterden uzun açıklama",
            Category = ExpenseCategory.Travel,
            Currency = Currency.TRY
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}