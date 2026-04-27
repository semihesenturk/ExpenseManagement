using Microsoft.EntityFrameworkCore;
using Expense.Domain.Entities;

namespace Expense.Application.Common.Interfaces;

public interface IExpenseDbContext
{
    DbSet<ExpenseRequest> ExpenseRequests { get; }
    public DbSet<Tenant> Tenants { get; }
    public DbSet<User> Users { get; }
    public DbSet<Approval> Approvals { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}