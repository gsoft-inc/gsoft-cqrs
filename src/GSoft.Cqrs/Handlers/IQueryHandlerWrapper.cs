namespace GSoft.Cqrs.Handlers;

internal interface IQueryHandlerWrapper<TResponse> : IHandlerWrapper
{
    public Task<TResponse> HandleAsync(IQuery<TResponse> query, CancellationToken cancellationToken);
}