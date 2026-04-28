using MediatR;
using Microsoft.Extensions.Logging;
using Expense.Application.Common.Interfaces;

namespace Expense.Application.Behaviors;

public class TenantValidationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    ILogger<TenantValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (currentUserService.TenantId is null)
        {
            logger.LogWarning("Missing tenant information for {RequestName}", typeof(TRequest).Name);
            throw new InvalidOperationException("Tenant context is required for this operation.");
        }

        return await next();
    }
}