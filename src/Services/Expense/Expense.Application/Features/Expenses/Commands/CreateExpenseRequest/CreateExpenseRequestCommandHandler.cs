using AutoMapper;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Domain.Entities;
using MediatR;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestCommandHandler 
    : IRequestHandler<CreateExpenseRequestCommand, CreateExpenseRequestDto>
{
    private readonly IExpenseRequestRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public CreateExpenseRequestCommandHandler(
        IExpenseRequestRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<CreateExpenseRequestDto> Handle(
        CreateExpenseRequestCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId 
                       ?? throw new InvalidOperationException("Tenant information is missing from request.");

        var userId = _currentUser.UserId 
                     ?? throw new InvalidOperationException("User information is missing from request.");

        var expense = new ExpenseRequest(tenantId, userId, request.Amount, request.Description);

        expense = await _repository.AddAsync(expense);

        return _mapper.Map<CreateExpenseRequestDto>(expense);
    }
}