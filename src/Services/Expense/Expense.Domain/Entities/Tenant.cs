using Expense.Domain.Common;

namespace Expense.Domain.Entities;

public class Tenant : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    // Navigation properties
    private readonly List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();
    
    private Tenant() { } 
    
    public Tenant(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
    public void AddUser(User user)
    {
        _users.Add(user);
        MarkAsUpdated();
    }
    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        MarkAsUpdated();
    }
}