using AutoMapper;
using MediatR;
using Expense.Application.Common.Interfaces;
using Expense.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequests;

public class GetExpenseRequestsQueryHandler(
    IExpenseDbContext context,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<GetExpenseRequestsQuery, PaginatedList<GetExpenseRequestsDto>>
{
    public async Task<PaginatedList<GetExpenseRequestsDto>> Handle(
        GetExpenseRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = context.ExpenseRequests.AsNoTracking();

        var userRoles = currentUser.Roles ?? [];
        var hasManagerialRole = userRoles.Contains("HR") ||
                                userRoles.Contains("Admin") ||
                                userRoles.Contains("Approver");

        if (!hasManagerialRole)
            query = query.Where(x => x.RequestedById == currentUser.UserId);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.StartDate.HasValue)
            query = query.Where(x => x.RequestDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.RequestDate <= request.EndDate.Value);

        query = query.OrderByDescending(x => x.RequestDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var items = mapper.Map<List<GetExpenseRequestsDto>>(entities);

        return new PaginatedList<GetExpenseRequestsDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}