using Expense.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;

public class GetExpenseRequestByIdQueryHandler(IExpenseDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetExpenseRequestByIdQuery, GetExpenseRequestByIdDto?>
{
    public async Task<GetExpenseRequestByIdDto?> Handle(GetExpenseRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId;
        if (!tenantId.HasValue) return null;
        
        var result = await context.ExpenseRequests
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