using Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Expense.Infrastructure.Persistence
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options)
            : base(options) { }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<User> Users => Set<User>();
        public DbSet<ExpenseRequest> ExpenseRequests => Set<ExpenseRequest>();
        public DbSet<Approval> Approvals => Set<Approval>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpenseDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}