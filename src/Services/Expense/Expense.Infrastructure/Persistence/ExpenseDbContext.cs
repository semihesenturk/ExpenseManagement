using Expense.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Expense.Domain.Entities;
using MassTransit;

public class ExpenseDbContext : DbContext, IExpenseDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

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
                _currentUserService.TenantId != null &&
                e.TenantId == _currentUserService.TenantId); 
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);
}