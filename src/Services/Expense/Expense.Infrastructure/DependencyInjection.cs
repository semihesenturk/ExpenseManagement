using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Infrastructure.Persistence;
using Expense.Infrastructure.Persistence.Repositories;
using Expense.Infrastructure.Services;
using MassTransit;
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
        
        services.AddScoped<IExpenseDbContext>(provider => provider.GetRequiredService<ExpenseDbContext>());
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        //MassTransit
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<ExpenseDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        // Repositories
        services.AddScoped<IExpenseRequestRepository, ExpenseRequestRepository>();
        
        //UoW
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}