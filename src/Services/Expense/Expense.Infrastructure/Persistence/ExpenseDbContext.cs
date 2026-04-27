using Expense.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Expense.Domain.Entities;

public class ExpenseDbContext : DbContext, IExpenseDbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ExpenseRequest> ExpenseRequests => Set<ExpenseRequest>();
    public DbSet<Approval> Approvals => Set<Approval>();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);
}