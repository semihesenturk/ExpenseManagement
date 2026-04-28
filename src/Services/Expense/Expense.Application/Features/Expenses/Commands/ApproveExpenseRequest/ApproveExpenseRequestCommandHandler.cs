using MediatR;
using MassTransit;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Domain.Enums;
using Shared.Contracts.Events;

namespace Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;

public class ApproveExpenseRequestCommandHandler : IRequestHandler<ApproveExpenseRequestCommand, Unit>
{
    private readonly IExpenseRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublishEndpoint _publishEndpoint;

    public ApproveExpenseRequestCommandHandler(
        IExpenseRequestRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Unit> Handle(ApproveExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
        var currentTenantId = _currentUser.TenantId;
        var userRoles = _currentUser.Roles ?? [];
        
        var expense = await _repository.GetByIdAsync(request.ExpenseRequestId, cancellationToken)
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
            await _publishEndpoint.Publish(new ExpenseApprovedEvent
            {
                ExpenseId = expense.Id,
                TenantId = expense.TenantId,
                ApprovedByUserId = currentUserId.ToString()
            }, cancellationToken);
        }
        
        await _repository.UpdateAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}