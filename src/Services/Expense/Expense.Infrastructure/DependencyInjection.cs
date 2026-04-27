using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Infrastructure.Persistence.Repositories;
using Expense.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Expense.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ExpenseDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        // Repositories
        services.AddScoped<IExpenseRequestRepository, ExpenseRequestRepository>();

        return services;
    }
}