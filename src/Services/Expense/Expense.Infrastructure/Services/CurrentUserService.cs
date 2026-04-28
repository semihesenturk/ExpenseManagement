using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Expense.Application.Common.Interfaces;

namespace Expense.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor contextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var claim = contextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "id");
            
            var userIdValue = claim?.Value;
            return Guid.TryParse(userIdValue, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var tenantClaim = contextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "TenantId" || c.Type.EndsWith("tenantid"));

            return Guid.TryParse(tenantClaim?.Value, out var id) ? id : null;
        }
    }
    
    public List<string> Roles
    {
        get
        {
            var claims = contextAccessor.HttpContext?.User?.Claims;
            
            if (claims == null) 
                return new List<string>();
            
            return claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }
    }
}