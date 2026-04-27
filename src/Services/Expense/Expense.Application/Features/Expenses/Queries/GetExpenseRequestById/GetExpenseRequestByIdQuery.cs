using MediatR;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;

public record GetExpenseRequestByIdQuery : IRequest<GetExpenseRequestByIdDto?>
{
    public Guid Id { get; init; }
}