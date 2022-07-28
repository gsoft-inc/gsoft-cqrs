namespace GSoft.Cqrs.Handlers;

internal interface IStreamHandlerWrapper<TResponse> : IHandlerWrapper
{
    IAsyncEnumerable<TResponse> StreamAsync(IStreamQuery<TResponse> query, CancellationToken cancellationToken);
}