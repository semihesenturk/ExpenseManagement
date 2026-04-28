using Expense.Application.Common.Interfaces;
using Expense.Domain.Entities;
using Shared.Contracts.Events;
using MassTransit;
using MediatR;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestCommandHandler : IRequestHandler<CreateExpenseRequestCommand, CreateExpenseRequestDto>
{
    private readonly IExpenseDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateExpenseRequestCommandHandler(
        IExpenseDbContext context, 
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateExpenseRequestDto> Handle(CreateExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;

        if (!userId.HasValue || !tenantId.HasValue)
            throw new UnauthorizedAccessException("Kullanıcı veya Tenant bilgisi bulunamadı.");


        var entity = new ExpenseRequest(
            tenantId.Value, 
            userId.Value, 
            request.Amount, 
            request.Description,
            request.Category,
            request.Currency
        );
        
        _context.ExpenseRequests.Add(entity);
        
        await _publishEndpoint.Publish(new ExpenseCreatedEvent
        {
            ExpenseId = entity.Id,
            TenantId = tenantId.Value,
            UserId = userId.Value,
            Amount = entity.Amount
        }, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new CreateExpenseRequestDto
        {
            Id = entity.Id,
            EmployeeId = userId.Value.ToString(),
            Amount = entity.Amount,
            Currency = entity.Currency.ToString(),
            Category = entity.Category.ToString(),
            Description = entity.Description,
            RequestDate = entity.RequestDate
        };
    }
}