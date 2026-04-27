using MediatR;
using Microsoft.Extensions.Logging;
using Expense.Application.Common.Interfaces;

namespace Expense.Application.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    public AuthorizationBehavior(
        ICurrentUserService currentUser,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            _logger.LogWarning("Unauthorized access attempt for {RequestName}", typeof(TRequest).Name);
            throw new UnauthorizedAccessException("You must be logged in to perform this action.");
        }

        return await next();
    }
}