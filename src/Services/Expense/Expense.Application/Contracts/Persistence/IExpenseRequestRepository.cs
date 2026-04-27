using Expense.Domain.Entities;

namespace Expense.Application.Contracts.Persistence;

public interface IExpenseRequestRepository
{
    Task<ExpenseRequest> AddAsync(ExpenseRequest expenseRequest);
    Task<ExpenseRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<ExpenseRequest>> GetByRequestedByIdAsync(Guid requestedById);
    Task UpdateAsync(ExpenseRequest expenseRequest, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id);
}