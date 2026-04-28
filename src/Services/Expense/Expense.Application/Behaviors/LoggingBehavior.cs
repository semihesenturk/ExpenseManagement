using MediatR;
using Microsoft.Extensions.Logging;
using Expense.Application.Common.Interfaces;
using System.Text.Json;

namespace Expense.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var user = _currentUser.UserId?.ToString() ?? "Anonymous";
        var tenant = _currentUser.TenantId?.ToString() ?? "NoTenant";

        _logger.LogInformation("Handling {RequestName} | Tenant: {TenantId} | User: {UserId} | Payload: {Payload}",
            requestName, tenant, user, JsonSerializer.Serialize(request));

        var response = await next();

        _logger.LogInformation("Completed {RequestName}", requestName);

        return response;
    }
}