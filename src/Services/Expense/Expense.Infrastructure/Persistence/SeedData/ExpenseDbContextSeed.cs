using Expense.Domain.Entities;
using Expense.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense.Infrastructure.Persistence.SeedData;

public static class ExpenseDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
        
        await context.Database.MigrateAsync();
        
        if (!await context.Tenants.AnyAsync())
        {
            var tenant = new Tenant("Izometri Bilişim", "Mülakat Test Ortamı"); 
            
            await context.Tenants.AddAsync(tenant);
            
            var tenantId = tenant.Id; 
            
            var adminUser = new User("admin@izometri.local", "Admin User", tenantId, UserRole.Admin);
            var approverUser = new User("approver@izometri.local", "Approver User", tenantId, UserRole.Approver);
            var employeeUser = new User("employee@izometri.local", "Employee User", tenantId, UserRole.Employee);

            tenant.AddUser(adminUser);
            tenant.AddUser(approverUser);
            tenant.AddUser(employeeUser);

            await context.SaveChangesAsync();
        }
    }
}