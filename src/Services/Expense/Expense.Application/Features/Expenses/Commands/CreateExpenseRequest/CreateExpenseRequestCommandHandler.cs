using Expense.Application.Common.Interfaces;
using Expense.Domain.Entities;
using Shared.Contracts.Events;
using MassTransit;
using MediatR;

namespace Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;

public class CreateExpenseRequestCommandHandler(
    IExpenseDbContext context,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreateExpenseRequestCommand, CreateExpenseRequestDto>
{
    public async Task<CreateExpenseRequestDto> Handle(CreateExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var tenantId = currentUserService.TenantId;

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
        
        context.ExpenseRequests.Add(entity);
        
        await publishEndpoint.Publish(new ExpenseCreatedEvent
        {
            ExpenseId = entity.Id,
            TenantId = tenantId.Value,
            UserId = userId.Value,
            Amount = entity.Amount
        }, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
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