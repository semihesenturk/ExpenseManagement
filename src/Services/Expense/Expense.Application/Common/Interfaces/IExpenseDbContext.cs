using Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Expense.Application.Common.Interfaces
{
    public interface IExpenseDbContext
    {
        DbSet<Tenant> Tenants { get; }
        DbSet<User> Users { get; }
        DbSet<ExpenseRequest> ExpenseRequests { get; }
        DbSet<Approval> Approvals { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}