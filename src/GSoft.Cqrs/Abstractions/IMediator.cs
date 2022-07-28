using GSoft.Cqrs.Abstractions.Events;

namespace GSoft.Cqrs;

/// <summary>
/// Execute `IQuery<T>` and `IStreamQuery<T>` based of handler previously registered.
/// </summary>
public interface IMediator
{
    Task<TResponse> HandleAsync<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TResponse> StreamAsync<TResponse>(IStreamQuery<TResponse> query, CancellationToken cancellationToken = default);

    Task<TResponse> DispatchAsync<TResponse>(ICommand<TResponse> request, CancellationToken cancellationToken = default);

    Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default);

    Task Publish(IEvent @event, CancellationToken cancellationToken = default);
}