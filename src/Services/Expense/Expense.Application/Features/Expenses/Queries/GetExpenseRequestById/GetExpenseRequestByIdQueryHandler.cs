using Expense.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;

public class GetExpenseRequestByIdQueryHandler : IRequestHandler<GetExpenseRequestByIdQuery, GetExpenseRequestByIdDto?>
{
    private readonly IExpenseDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetExpenseRequestByIdQueryHandler(IExpenseDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<GetExpenseRequestByIdDto?> Handle(GetExpenseRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (!tenantId.HasValue) return null;
        
        var result = await _context.ExpenseRequests
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.TenantId == tenantId.Value)
            .Select(x => new
            {
                x.Id,
                x.Amount,
                x.Description,
                StatusString = x.Status.ToString(),
                x.RequestDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null) return null;

        return new GetExpenseRequestByIdDto
        {
            Id = result.Id,
            Amount = result.Amount,
            Description = result.Description,
            Status = result.StatusString,
            RequestDate = result.RequestDate
        };
    }
}