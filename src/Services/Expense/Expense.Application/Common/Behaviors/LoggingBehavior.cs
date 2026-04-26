using MediatR;
using Microsoft.Extensions.Logging;

namespace Expense.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;

        public LoggingBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestName} with data {@Request}", typeof(TRequest).Name, request);
            var response = await next();
            _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
            return response;
        }
    }
}