using MediatR;
using MassTransit;
using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Shared.Contracts.Events;

namespace Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;

public class RejectExpenseRequestCommandHandler : IRequestHandler<RejectExpenseRequestCommand, Unit>
{
    private readonly IExpenseRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublishEndpoint _publishEndpoint;

    public RejectExpenseRequestCommandHandler(
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

    public async Task<Unit> Handle(RejectExpenseRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
        var currentTenantId = _currentUser.TenantId;
        var userRoles = _currentUser.Roles ?? [];
        
        var expense = await _repository.GetByIdAsync(request.ExpenseRequestId, cancellationToken)
                      ?? throw new KeyNotFoundException("Harcama talebi bulunamadı.");
        
        if (expense.TenantId != currentTenantId)
            throw new UnauthorizedAccessException("Bu harcamaya erişim yetkiniz yok.");
        
        var hasManagerialRole = userRoles.Contains("HR") || userRoles.Contains("Admin") || userRoles.Contains("Approver");
        if (!hasManagerialRole)
            throw new UnauthorizedAccessException("Bu harcamayı reddetme yetkiniz yok.");

        expense.Reject(currentUserId, request.Note);
        
        await _publishEndpoint.Publish(new ExpenseRejectedEvent
        {
            ExpenseId = expense.Id,
            TenantId = expense.TenantId,
            RejectedByUserId = currentUserId.ToString(),
            Reason = request.Note
        }, cancellationToken);
        
        await _repository.UpdateAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}