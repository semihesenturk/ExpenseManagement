using MediatR;
using Expense.Application.Common.Models;
using Expense.Domain.Enums;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequests;

public record GetExpenseRequestsQuery : IRequest<PaginatedList<GetExpenseRequestsDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    
    public ExpenseStatus? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}