using Expense.Application.Common.Interfaces;
using Expense.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Expense.Infrastructure.Persistence;

public class ExpenseDbContext(DbContextOptions<ExpenseDbContext> options, ICurrentUserService currentUserService)
    : DbContext(options), IExpenseDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ExpenseRequest> ExpenseRequests => Set<ExpenseRequest>();
    public DbSet<Approval> Approvals => Set<Approval>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpenseDbContext).Assembly);
        
        modelBuilder.Entity<ExpenseRequest>()
            .HasQueryFilter(e => 
                !e.IsDeleted && 
                currentUserService.TenantId != null &&
                e.TenantId == currentUserService.TenantId); 
        
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => 
                !u.IsDeleted &&
                currentUserService.TenantId != null &&
                u.TenantId == currentUserService.TenantId);
        
        modelBuilder.Entity<Approval>()
            .HasQueryFilter(a => 
                !a.IsDeleted &&
                currentUserService.TenantId != null &&
                a.ExpenseRequest!.TenantId == currentUserService.TenantId);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);
}