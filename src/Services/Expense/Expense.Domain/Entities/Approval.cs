using Expense.Domain.Common;
using Expense.Domain.Enums;

namespace Expense.Domain.Entities
{
    public class Approval : BaseEntity
    {
        public Guid ExpenseRequestId { get; private set; }
        public Guid ApproverId { get; private set; }
        public ApprovalStatus Status { get; private set; }
        public string? Note { get; private set; }
        public DateTime DecisionDate { get; private set; } = DateTime.UtcNow;

        // Navigation property
        public ExpenseRequest? ExpenseRequest { get; private set; }

        private Approval() { }

        public Approval(Guid expenseRequestId, Guid approverId, ApprovalStatus status, string? note)
        {
            ExpenseRequestId = expenseRequestId;
            ApproverId = approverId;
            Status = status;
            Note = note;
        }
    }
}