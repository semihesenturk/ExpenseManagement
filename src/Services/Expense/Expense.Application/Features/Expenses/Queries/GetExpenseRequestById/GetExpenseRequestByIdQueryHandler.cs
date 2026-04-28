using AutoMapper;
using Expense.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;

public class GetExpenseRequestByIdQueryHandler(
    IExpenseDbContext context,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<GetExpenseRequestByIdQuery, GetExpenseRequestByIdDto?>
{
    public async Task<GetExpenseRequestByIdDto?> Handle(
        GetExpenseRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUser.TenantId;
        if (!tenantId.HasValue) return null;

        var entity = await context.ExpenseRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.Id && x.TenantId == tenantId.Value,
                cancellationToken);
        
        return entity is null ? null : mapper.Map<GetExpenseRequestByIdDto>(entity);
    }
}