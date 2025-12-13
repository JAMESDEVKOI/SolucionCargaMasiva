using MediatR;
using Microsoft.Extensions.Logging;

namespace FileControl.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation(
                "Ejecutando el request: {RequestName}",
                requestName);

            try
            {
                var response = await next();

                _logger.LogInformation(
                    "El request {RequestName} se ejecutó correctamente",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "El request {RequestName} tuvo errores",
                    requestName);
                throw;
            }
        }
    }
}
