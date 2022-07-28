using GSoft.Cqrs.Abstractions.Middlewares;

namespace GSoft.Cqrs.Handlers;

internal sealed class QueryHandlerWrapper<TQueryHandler, TQuery, TResponse> : IQueryHandlerWrapper<TResponse>
    where TQuery : IQuery<TResponse>
    where TQueryHandler : IQueryHandler<TQuery, TResponse>
{
    private readonly IEnumerable<IRequestMiddleware<TQuery, TResponse>> _queryHandlerMiddlewares;
    private readonly TQueryHandler _handler;

    public QueryHandlerWrapper(IEnumerable<IRequestMiddleware<TQuery, TResponse>> queryHandlerMiddlewares, TQueryHandler handler)
    {
        this._queryHandlerMiddlewares = queryHandlerMiddlewares.Reverse();
        this._handler = handler;
    }

    public Task<TResponse> HandleAsync(IQuery<TResponse> query, CancellationToken cancellationToken)
    {
        Task<TResponse> Handler() => this._handler.HandleAsync((TQuery)query, cancellationToken);

        var next = this._queryHandlerMiddlewares
            .Aggregate(
                (RequestHandlerDelegate<TResponse>)Handler,
                (next, middleware) => () => middleware.HandleAsync(next, (TQuery)query, cancellationToken));

        return next();
    }
}