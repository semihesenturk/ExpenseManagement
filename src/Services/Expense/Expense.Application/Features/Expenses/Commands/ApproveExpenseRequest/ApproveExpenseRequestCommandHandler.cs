using MediatR;
using MassTransit;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Domain.Enums;
using Shared.Contracts.Events; // Kendi namespace'lerine dikkat et

namespace Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;

public class ApproveExpenseRequestCommandHandler : IRequestHandler<ApproveExpenseRequestCommand, Unit>
{
    private readonly IExpenseRequestRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublishEndpoint _publishEndpoint;

    public ApproveExpenseRequestCommandHandler(
        IExpenseRequestRepository repository,
        ICurrentUserService currentUser,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _currentUser = currentUser;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Unit> Handle(ApproveExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var expense = await _repository.GetByIdAsync(request.ExpenseRequestId, cancellationToken)
            ?? throw new KeyNotFoundException("Harcama bulunamadı.");
        
        var userRoles = _currentUser.Roles ?? new List<string>();
        var isHr = userRoles.Contains("HR");
        var isAdmin = userRoles.Contains("Admin");

        if (!isHr && !isAdmin)
            throw new UnauthorizedAccessException("Bu işlemi yapmaya yetkiniz yok.");
        
        if (expense.Amount <= 5000)
        {
            if (!isHr) throw new UnauthorizedAccessException("5000 TL altı harcamaları sadece HR onaylayabilir.");
            expense.Approve(request.ApproverId, request.Note); 
        }
        else
        {
            if (expense.Status == ExpenseStatus.Pending)
            {
                if (!isHr) throw new UnauthorizedAccessException("İlk onayı HR vermelidir.");
                
                expense.SendToAdminApproval(request.ApproverId, request.Note);
                
                await _repository.UpdateAsync(expense, cancellationToken);
                return Unit.Value;
            }
            else if (expense.Status == ExpenseStatus.PendingAdminApproval)
            {
                if (!isAdmin) throw new UnauthorizedAccessException("Bu harcama Admin onayı bekliyor.");
                
                expense.Approve(request.ApproverId, request.Note);
            }
        }


        await _repository.UpdateAsync(expense, cancellationToken);
        
        if (expense.Status == ExpenseStatus.Approved)
        {
            await _publishEndpoint.Publish(new ExpenseApprovedEvent
            {
                ExpenseId = expense.Id,
                TenantId = expense.TenantId,
                ApprovedByUserId = request.ApproverId.ToString()
            }, cancellationToken);
        }

        return Unit.Value;
    }
}