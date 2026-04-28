using MediatR;
using Microsoft.Extensions.Logging;
using Expense.Application.Common.Interfaces;

namespace Expense.Application.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            logger.LogWarning("Unauthorized access attempt for {RequestName}", typeof(TRequest).Name);
            throw new UnauthorizedAccessException("You must be logged in to perform this action.");
        }

        return await next();
    }
}