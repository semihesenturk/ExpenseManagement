using AutoMapper;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Domain.Entities;
using MassTransit;
using MediatR;
using Shared.Contracts.Events;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestCommandHandler 
    : IRequestHandler<CreateExpenseRequestCommand, CreateExpenseRequestDto>
{
    private readonly IExpenseRequestRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateExpenseRequestCommandHandler(
        IExpenseRequestRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateExpenseRequestDto> Handle(
        CreateExpenseRequestCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId.Value;

        var userId = _currentUser.UserId.Value;

        var expense = new ExpenseRequest(tenantId, userId, request.Amount, request.Description);

        expense = await _repository.AddAsync(expense);
        
        //Throw event for notification service
        await _publishEndpoint.Publish(new ExpenseCreatedEvent
        {
            ExpenseId = expense.Id,
            TenantId = tenantId,
            UserId = userId,
            Amount = request.Amount
        }, cancellationToken);

        return _mapper.Map<CreateExpenseRequestDto>(expense);
    }
}