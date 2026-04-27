using MediatR;
using Microsoft.Extensions.Logging;
using Expense.Application.Common.Interfaces;

namespace Expense.Application.Behaviors;

public class TenantValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TenantValidationBehavior<TRequest, TResponse>> _logger;

    public TenantValidationBehavior(
        ICurrentUserService currentUserService,
        ILogger<TenantValidationBehavior<TRequest, TResponse>> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.TenantId is null)
        {
            _logger.LogWarning("Missing tenant information for {RequestName}", typeof(TRequest).Name);
            throw new InvalidOperationException("Tenant context is required for this operation.");
        }

        return await next();
    }
}