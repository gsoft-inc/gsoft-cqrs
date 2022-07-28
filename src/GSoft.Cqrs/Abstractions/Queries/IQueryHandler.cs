namespace GSoft.Cqrs;

public interface IQueryHandler<in TQuery, TResponse> : IHandler
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}