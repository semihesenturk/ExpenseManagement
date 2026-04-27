namespace Expense.Domain.Enums
{
    public enum ExpenseStatus
    {
        Draft = 1,
        Pending = 2,
        PendingAdminApproval = 3,
        Approved = 4,
        Rejected = 5
    }
}