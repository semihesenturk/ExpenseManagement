using System.Net;
using System.Net.Http.Json;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Domain.Enums;
using Expense.IntegrationTests.Factories;
using FluentAssertions;

namespace Expense.IntegrationTests.ControllerTests;

public class ExpensesControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private CreateExpenseRequestCommand ValidCommand() => new()
    {
        Amount = 1500,
        Description = "Yirmi karakterden uzun geçerli açıklama",
        Category = ExpenseCategory.Travel,
        Currency = Currency.TRY
    };


    [Fact]
    public async Task CreateExpense_WithValidCommand_ShouldReturn200()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(role: "Employee");

        // Act
        var response = await client.PostAsJsonAsync("/api/Expenses", ValidCommand());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CreateExpenseRequestDto>();
        result.Should().NotBeNull();
        result!.Amount.Should().Be(1500);
        result.Category.Should().Be("Travel");
        result.Currency.Should().Be("TRY");
    }
    
    [Fact]
    public async Task GetExpenses_WithAuth_ShouldReturn200()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(role: "HR");

        // Act
        var response = await client.GetAsync("/api/Expenses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

}