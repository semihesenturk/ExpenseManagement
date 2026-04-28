using Expense.Application.Common.Interfaces;

namespace Expense.Infrastructure.Persistence;

public class UnitOfWork(ExpenseDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}