using MediatR;
using Microsoft.Extensions.Logging;

namespace Expense.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling {RequestName} with data {@Request}", typeof(TRequest).Name, request);
            var response = await next();
            logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
            return response;
        }
    }
}