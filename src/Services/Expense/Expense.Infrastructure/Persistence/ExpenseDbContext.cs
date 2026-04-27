using Expense.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Expense.Domain.Entities;
using MassTransit;

public class ExpenseDbContext : DbContext, IExpenseDbContext
{
    private readonly ICurrentTenantService _tenantService;
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
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
                _tenantService.TenantId.HasValue && 
                e.TenantId == _tenantService.TenantId.Value); 
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);
}