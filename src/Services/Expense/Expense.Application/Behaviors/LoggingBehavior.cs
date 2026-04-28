using MediatR;
using Microsoft.Extensions.Logging;
using Expense.Application.Common.Interfaces;
using System.Text.Json;

namespace Expense.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var user = currentUser.UserId?.ToString() ?? "Anonymous";
        var tenant = currentUser.TenantId?.ToString() ?? "NoTenant";

        logger.LogInformation("Handling {RequestName} | Tenant: {TenantId} | User: {UserId} | Payload: {Payload}",
            requestName, tenant, user, JsonSerializer.Serialize(request));

        var response = await next();

        logger.LogInformation("Completed {RequestName}", requestName);

        return response;
    }
}