using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Tests;

public class GenericMiddleWare<TRequest, TResponse> : BaseMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public GenericMiddleWare(ILogger<GenericMiddleWare<TRequest, TResponse>> logger)
        : base(logger)
    {
    }
}
