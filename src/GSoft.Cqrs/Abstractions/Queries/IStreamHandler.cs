namespace GSoft.Cqrs;

public interface IStreamHandler<in TStreamQuery, out TResponse> : IHandler
    where TStreamQuery : IStreamQuery<TResponse>
{
    IAsyncEnumerable<TResponse> StreamAsync(TStreamQuery query, CancellationToken cancellationToken);
}