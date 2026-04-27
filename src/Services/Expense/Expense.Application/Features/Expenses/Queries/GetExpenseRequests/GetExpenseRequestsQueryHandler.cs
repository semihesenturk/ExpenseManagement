using AutoMapper;
using Expense.Application.Contracts.Persistence;
using MediatR;

namespace Expense.Application.Features.Expenses.Queries.GetExpenseRequests;

public class GetExpenseRequestsQueryHandler 
    : IRequestHandler<GetExpenseRequestsQuery, List<GetExpenseRequestsDto>>
{
    private readonly IExpenseRequestRepository _repository;
    private readonly IMapper _mapper;

    public GetExpenseRequestsQueryHandler(IExpenseRequestRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<GetExpenseRequestsDto>> Handle(
        GetExpenseRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var expenses = await _repository.GetByRequestedByIdAsync(request.RequestedById);
        return expenses.Select(_mapper.Map<GetExpenseRequestsDto>).ToList();
    }
}