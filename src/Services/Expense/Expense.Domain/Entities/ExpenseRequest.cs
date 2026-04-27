using Expense.Domain.Common;
using Expense.Domain.Enums;

namespace Expense.Domain.Entities
{
    public class ExpenseRequest : AggregateRoot
    {
        public Guid TenantId { get; private set; }
        public Guid RequestedById { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; }
        public DateTime RequestDate { get; private set; } = DateTime.UtcNow;
        public ExpenseStatus Status { get; private set; } = ExpenseStatus.Pending;

        // Navigation properties
        public User? RequestedBy { get; private set; }
        private readonly List<Approval> _approvals = new();
        public IReadOnlyCollection<Approval> Approvals => _approvals.AsReadOnly();

        private ExpenseRequest() { } // EF Core için

        public ExpenseRequest(Guid tenantId, Guid requestedById, decimal amount, string description)
        {
            TenantId = tenantId;
            RequestedById = requestedById;
            Amount = amount;
            Description = description;
        }

        public void Approve(Guid approverId, string? note = null)
        {
            if (Status == ExpenseStatus.Approved)
                throw new InvalidOperationException("Request already approved.");

            Status = ExpenseStatus.Approved;
            AddApproval(approverId, ApprovalStatus.Approved, note);
            MarkAsUpdated();
        }

        public void Reject(Guid approverId, string? note = null)
        {
            if (Status == ExpenseStatus.Rejected)
                throw new InvalidOperationException("Request already rejected.");

            Status = ExpenseStatus.Rejected;
            AddApproval(approverId, ApprovalStatus.Rejected, note);
            MarkAsUpdated();
        }

        private void AddApproval(Guid approverId, ApprovalStatus status, string? note)
        {
            var approval = new Approval(Id, approverId, status, note);
            _approvals.Add(approval);
        }
        
        public void SendToAdminApproval(Guid approverId, string? note = null)
        {
            if (Status != ExpenseStatus.Pending)
                throw new InvalidOperationException("Request is not in pending state.");

            Status = ExpenseStatus.PendingAdminApproval;
            AddApproval(approverId, ApprovalStatus.Approved, note);
            MarkAsUpdated();
        }
    }
}
