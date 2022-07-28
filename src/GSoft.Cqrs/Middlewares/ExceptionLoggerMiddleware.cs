using GSoft.Cqrs.Abstractions.Middlewares;

using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Middlewares;

public class ExceptionLoggerMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionLoggerMiddleware<TRequest, TResponse>> _logger;

    public ExceptionLoggerMiddleware(ILogger<ExceptionLoggerMiddleware<TRequest, TResponse>> logger)
    {
        this._logger = logger;
    }

    public async Task<TResponse> HandleAsync(RequestHandlerDelegate<TResponse> next, TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Unhandled exception in {RequestName}", request.GetType().FullName);
            throw;
        }
    }
}
