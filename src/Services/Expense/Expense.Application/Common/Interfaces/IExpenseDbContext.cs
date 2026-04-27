namespace Expense.Application.Common.Interfaces;

public interface IExpenseDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}