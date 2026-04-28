using MediatR;
using MassTransit;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Domain.Enums;
using Shared.Contracts.Events;

namespace Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;

public class ApproveExpenseRequestCommandHandler(
    IExpenseRequestRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ApproveExpenseRequestCommand, Unit>
{
    public async Task<Unit> Handle(ApproveExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.UserId ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
        var currentTenantId = currentUser.TenantId;
        var userRoles = currentUser.Roles ?? [];
        
        var expense = await repository.GetByIdAsync(request.ExpenseRequestId, cancellationToken)
            ?? throw new KeyNotFoundException("Harcama bulunamadı.");
        
        if (expense.TenantId != currentTenantId)
            throw new UnauthorizedAccessException("Bu harcamaya erişim yetkiniz yok.");

        var isHrOrApprover = userRoles.Contains("HR") || userRoles.Contains("Approver");
        var isAdmin = userRoles.Contains("Admin");

        if (!isHrOrApprover && !isAdmin)
            throw new UnauthorizedAccessException("Bu işlemi yapmaya yetkiniz yok.");
        
        if (expense.Amount <= 5000)
        {
            if (!isHrOrApprover) throw new UnauthorizedAccessException("5000 TL altı harcamaları sadece HR/Approver onaylayabilir.");
            expense.Approve(currentUserId, request.Note); 
        }
        else
        {
            if (expense.Status == ExpenseStatus.Pending)
            {
                if (!isHrOrApprover) throw new UnauthorizedAccessException("5000 TL üzeri harcamalar için ilk onayı HR/Approver vermelidir.");
                expense.SendToAdminApproval(currentUserId, request.Note);
            }
            else if (expense.Status == ExpenseStatus.PendingAdminApproval)
            {
                if (!isAdmin) throw new UnauthorizedAccessException("Bu harcama Admin onayı bekliyor.");
                expense.Approve(currentUserId, request.Note);
            }
        }
        
        if (expense.Status == ExpenseStatus.Approved)
        {
            await publishEndpoint.Publish(new ExpenseApprovedEvent
            {
                ExpenseId = expense.Id,
                TenantId = expense.TenantId,
                ApprovedByUserId = currentUserId.ToString()
            }, cancellationToken);
        }
        
        await repository.UpdateAsync(expense, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}