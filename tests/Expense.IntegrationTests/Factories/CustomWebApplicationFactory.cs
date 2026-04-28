using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Infrastructure.Persistence;
using Expense.Infrastructure.Persistence.Repositories;
using Expense.Infrastructure.Services;
using Expense.IntegrationTests.Setup;
using MassTransit;

namespace Expense.IntegrationTests.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Guid TestTenantId { get; } = Guid.NewGuid();
    public Guid TestUserId { get; } = Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // InMemory DbContext
            services.AddDbContext<ExpenseDbContext>(options =>
                options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}"));

            // Gerekli bağımlılıkları manuel register et
            services.AddScoped<IExpenseDbContext>(
                provider => provider.GetRequiredService<ExpenseDbContext>());
            services.AddScoped<IExpenseRequestRepository, ExpenseRequestRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // MassTransit test harness
            services.AddMassTransitTestHarness();

            // Auth bypass
            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "TestScheme", options => { });

            // DB şeması oluştur
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public HttpClient CreateAuthenticatedClient(
        Guid? tenantId = null,
        Guid? userId = null,
        string role = "HR")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-TenantId", (tenantId ?? TestTenantId).ToString());
        client.DefaultRequestHeaders.Add("X-Test-UserId", (userId ?? TestUserId).ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme");
        return client;
    }
}