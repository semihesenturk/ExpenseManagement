using Expense.Domain.Common;
using Expense.Domain.Enums;

namespace Expense.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; }
    public string FullName { get; private set; }
    public Guid TenantId { get; private set; }
    public UserRole Role { get; private set; }
    // FK ilişki
    public Tenant? Tenant { get; private set; }
    
    private User() { }
    public User(string email, string fullName, Guid tenantId, UserRole role)
    {
        Email = email;
        FullName = fullName;
        TenantId = tenantId;
        Role = role;
    }
    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        MarkAsUpdated();
    }
}