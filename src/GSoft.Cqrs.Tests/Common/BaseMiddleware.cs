using GSoft.Cqrs.Abstractions.Middlewares;

using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Tests;

public class BaseMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected BaseMiddleware(ILogger<BaseMiddleware<TRequest, TResponse>> logger)
    {
        this.Logger = logger;
    }

    protected ILogger<BaseMiddleware<TRequest, TResponse>> Logger { get; }

    public async Task<TResponse> HandleAsync(RequestHandlerDelegate<TResponse> next, TRequest request, CancellationToken cancellationToken)
    {
        this.Logger.LogInformation("Middleware {MiddlewareType} pre-processing", this.GetType().FullName);
        var value = await next();
        this.Logger.LogInformation("Middleware {MiddlewareType} post-processing", this.GetType().FullName);

        return value;
    }
}
