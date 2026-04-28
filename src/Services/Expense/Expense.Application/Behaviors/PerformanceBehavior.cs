using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Expense.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsed = _timer.ElapsedMilliseconds;

        if (elapsed > 500)
        {
            logger.LogWarning(
                "Long running request: {RequestName} took {Elapsed}ms.",
                typeof(TRequest).Name, elapsed);
        }

        return response;
    }
}