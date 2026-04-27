using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Expense.Application.Common.Interfaces;

namespace Expense.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var claim = _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            var userIdValue = claim?.Value;
            
            return Guid.TryParse(userIdValue, out var id) ? id : null;
        }
    }
    
    public List<string> Roles
    {
        get
        {
            var claims = _contextAccessor.HttpContext?.User?.Claims;
            
            if (claims == null) 
                return new List<string>();

            return claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }
    }
}