using MediatR;
using MassTransit;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Shared.Contracts.Events;

namespace Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;

public class RejectExpenseRequestCommandHandler(
    IExpenseRequestRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<RejectExpenseRequestCommand, Unit>
{
    public async Task<Unit> Handle(RejectExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.UserId ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
        var currentTenantId = currentUser.TenantId;
        var userRoles = currentUser.Roles ?? [];
        
        var expense = await repository.GetByIdAsync(request.ExpenseRequestId, cancellationToken)
                      ?? throw new KeyNotFoundException("Harcama talebi bulunamadı.");
        
        if (expense.TenantId != currentTenantId)
            throw new UnauthorizedAccessException("Bu harcamaya erişim yetkiniz yok.");
        
        var hasManagerialRole = userRoles.Contains("HR") || userRoles.Contains("Admin") || userRoles.Contains("Approver");
        if (!hasManagerialRole)
            throw new UnauthorizedAccessException("Bu harcamayı reddetme yetkiniz yok.");

        expense.Reject(currentUserId, request.Note);
        
        await publishEndpoint.Publish(new ExpenseRejectedEvent
        {
            ExpenseId = expense.Id,
            TenantId = expense.TenantId,
            RejectedByUserId = currentUserId.ToString(),
            Reason = request.Note
        }, cancellationToken);
        
        await repository.UpdateAsync(expense, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}