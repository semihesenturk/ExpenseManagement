using Expense.Application.Contracts.Persistence;
using Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Expense.Infrastructure.Persistence.Repositories;

public class ExpenseRequestRepository : IExpenseRequestRepository
{
    private readonly ExpenseDbContext _context;

    public ExpenseRequestRepository(ExpenseDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseRequest> AddAsync(ExpenseRequest expenseRequest)
    {
        await _context.ExpenseRequests.AddAsync(expenseRequest);
        await _context.SaveChangesAsync();
        return expenseRequest;
    }

    public async Task<ExpenseRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ExpenseRequests
            .AsNoTracking()
            .Include(x => x.Approvals) 
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ExpenseRequest>> GetByRequestedByIdAsync(Guid requestedById)
        => await _context.ExpenseRequests
            .Where(x => x.RequestedById == requestedById)
            .Include(x => x.Approvals)
            .ToListAsync();
    

    public async Task UpdateAsync(ExpenseRequest expenseRequest , CancellationToken cancellationToken)
    {
        _context.ExpenseRequests.Update(expenseRequest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.ExpenseRequests.FindAsync(id);
        if (entity is null) return;

        _context.ExpenseRequests.Remove(entity);
        await _context.SaveChangesAsync();
    }
}