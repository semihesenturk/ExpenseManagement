using Expense.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Expense.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentTenantService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var tenantHeader = _contextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            return Guid.TryParse(tenantHeader, out var id) ? id : null;
        }
    }
}