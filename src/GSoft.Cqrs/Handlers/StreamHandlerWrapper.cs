using System.Runtime.CompilerServices;

using GSoft.Cqrs.Abstractions.Middlewares;

namespace GSoft.Cqrs.Handlers;

internal sealed class StreamHandlerWrapper<TStreamHandler, TStreamQuery, TResponse> : IStreamHandlerWrapper<TResponse>
    where TStreamQuery : IStreamQuery<TResponse>
    where TStreamHandler : IStreamHandler<TStreamQuery, TResponse>
{
    private readonly IEnumerable<IStreamMiddleware<TStreamQuery, TResponse>> _queryHandlerMiddlewares;
    private readonly TStreamHandler _handler;

    public StreamHandlerWrapper(IEnumerable<IStreamMiddleware<TStreamQuery, TResponse>> queryHandlerMiddlewares, TStreamHandler handler)
    {
        this._queryHandlerMiddlewares = queryHandlerMiddlewares.Reverse();
        this._handler = handler;
    }

    public async IAsyncEnumerable<TResponse> StreamAsync(IStreamQuery<TResponse> query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<TResponse> Handler() => this._handler.StreamAsync((TStreamQuery)query, cancellationToken);
        var items = this._queryHandlerMiddlewares
            .Aggregate(
                (StreamHandlerDelegate<TResponse>)Handler,
                (next, middleware) =>
                    () => middleware.StreamAsync(
                        () => NextWrapper(
                                next(),
                                cancellationToken),
                        (TStreamQuery)query,
                        cancellationToken))();

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> NextWrapper<T>(
        IAsyncEnumerable<T> items,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cancellable = items
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);
        await foreach (var item in cancellable)
        {
            yield return item;
        }
    }
}