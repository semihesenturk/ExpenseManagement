using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using FluentValidation.TestHelper;

namespace Expense.UnitTests.Application.Features.Expenses.Commands;

public class CreateExpenseRequestCommandValidatorTests
{
    private readonly CreateExpenseRequestCommandValidator _validator;

    public CreateExpenseRequestCommandValidatorTests()
    {
        // Gerçek validator nesnemizi ayağa kaldırıyoruz
        _validator = new CreateExpenseRequestCommandValidator();
    }

    [Fact]
    public void Validate_WhenAmountIsZeroOrLess_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateExpenseRequestCommand(0, "Yeni Monitör");

        // Act
        var result = _validator.TestValidate(command);

        // Assert: Beklenen hata mesajını seninkiyle birebir eşledik
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Tutar sıfırdan büyük olmalıdır."); 
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateExpenseRequestCommand(1500, string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateExpenseRequestCommand(1500, "Geçerli Bir Harcama Açıklaması");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}