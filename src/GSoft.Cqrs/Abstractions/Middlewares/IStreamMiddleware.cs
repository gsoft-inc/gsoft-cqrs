namespace GSoft.Cqrs.Abstractions.Middlewares;

public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<out TResponse>();

public interface IStreamMiddleware<in TStream, TResponse>
    where TStream : IStreamQuery<TResponse>
{
    IAsyncEnumerable<TResponse> StreamAsync(StreamHandlerDelegate<TResponse> next, TStream streamQuery, CancellationToken cancellationToken);
}
