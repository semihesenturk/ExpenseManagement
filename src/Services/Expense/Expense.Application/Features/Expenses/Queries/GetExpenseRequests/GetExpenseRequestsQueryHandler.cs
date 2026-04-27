using MediatR;
using Expense.Application.Common.Interfaces;
using Expense.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequests;

public class GetExpenseRequestsQueryHandler : IRequestHandler<GetExpenseRequestsQuery, PaginatedList<GetExpenseRequestsDto>>
{
    private readonly IExpenseDbContext _context; 
    private readonly ICurrentUserService _currentUser;

    public GetExpenseRequestsQueryHandler(IExpenseDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<GetExpenseRequestsDto>> Handle(GetExpenseRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ExpenseRequests.AsNoTracking();
        
        var userRoles = _currentUser.Roles ?? [];
        
        var hasManagerialRole = userRoles.Contains("HR") || 
                                userRoles.Contains("Admin") || 
                                userRoles.Contains("Approver");

        if (!hasManagerialRole)
        {
            query = query.Where(x => x.RequestedById == _currentUser.UserId);
        }
        
        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.StartDate.HasValue)
            query = query.Where(x => x.RequestDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.RequestDate <= request.EndDate.Value);
        
        query = query.OrderByDescending(x => x.RequestDate);
        

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new GetExpenseRequestsDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Description = x.Description,
                Status = x.Status.ToString(),
                RequestDate = x.RequestDate
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<GetExpenseRequestsDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}