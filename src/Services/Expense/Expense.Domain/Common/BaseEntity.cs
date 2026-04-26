namespace Expense.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public void MarkAsUpdated()
    {
        UpdatedDate = DateTime.UtcNow;
    }
    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedDate = DateTime.UtcNow;
    }
}