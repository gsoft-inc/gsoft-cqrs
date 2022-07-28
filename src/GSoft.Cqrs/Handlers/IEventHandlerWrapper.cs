using GSoft.Cqrs.Abstractions.Events;

namespace GSoft.Cqrs.Handlers;

public interface IEventHandlerWrapper : IHandlerWrapper
{
    public Task HandleAsync(IEvent @event, Func<IEnumerable<Func<IEvent, CancellationToken, Task>>, IEvent, CancellationToken, Task> publish, CancellationToken cancellationToken);
}
