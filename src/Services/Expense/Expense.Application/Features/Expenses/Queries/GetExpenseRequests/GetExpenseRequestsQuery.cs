using MediatR;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequests;

public record GetExpenseRequestsQuery(Guid RequestedById) 
    : IRequest<List<GetExpenseRequestsDto>>;