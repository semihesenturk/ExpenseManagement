using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Expense.IntegrationTests.Setup;

namespace Expense.IntegrationTests.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 1. Uygulamanın "Testing" ortamında çalışacağını belirtiyoruz
        // Böylece Program.cs'deki PostgreSQL koduna hiç girilmeyecek
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 2. InMemory DbContext'i burada ekliyoruz
            services.AddDbContext<ExpenseDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });

            // 3. Auth Bypass (TestAuthHandler üzerinden)
            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            // 4. Veritabanı şemasını oluşturma
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
            
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }
}