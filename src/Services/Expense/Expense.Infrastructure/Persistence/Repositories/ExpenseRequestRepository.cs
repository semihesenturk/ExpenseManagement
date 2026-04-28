using Expense.Application.Contracts.Persistence;
using Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Expense.Infrastructure.Persistence.Repositories;

public class ExpenseRequestRepository(ExpenseDbContext context) : IExpenseRequestRepository
{
    public async Task<ExpenseRequest> AddAsync(ExpenseRequest expenseRequest)
    {
        await context.ExpenseRequests.AddAsync(expenseRequest);
        await context.SaveChangesAsync();
        return expenseRequest;
    }

    public async Task<ExpenseRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.ExpenseRequests
            .AsNoTracking()
            .Include(x => x.Approvals) 
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ExpenseRequest>> GetByRequestedByIdAsync(Guid requestedById)
        => await context.ExpenseRequests
            .Where(x => x.RequestedById == requestedById)
            .Include(x => x.Approvals)
            .ToListAsync();
    

    public async Task UpdateAsync(ExpenseRequest expenseRequest , CancellationToken cancellationToken)
    {
        var entry = context.Entry(expenseRequest);
        if (entry.State == EntityState.Detached)
        {
            context.ExpenseRequests.Attach(expenseRequest);
        }
        
        entry.State = EntityState.Modified;
    }

    public async Task DeleteAsync(Guid id, Guid deletedBy)
    {
        var entity = await context.ExpenseRequests.FindAsync(id);
        if (entity is null) return;

        entity.SoftDelete(deletedBy);
        await context.SaveChangesAsync();
    }
}