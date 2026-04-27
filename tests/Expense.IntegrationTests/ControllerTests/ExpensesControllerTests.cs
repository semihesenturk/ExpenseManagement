using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.IntegrationTests.Factories;
using FluentAssertions;

namespace Expense.IntegrationTests.ControllerTests;

public class ExpensesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpensesControllerTests(CustomWebApplicationFactory factory)
    {
        // API'yi ayağa kaldırıp, istek atacağımız HttpClient'ı oluşturuyoruz
        _client = factory.CreateClient();
        
        // Bütün isteklere otomatik TestScheme yetkisi ekliyoruz
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }
}