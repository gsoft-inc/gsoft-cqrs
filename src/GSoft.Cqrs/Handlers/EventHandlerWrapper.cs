using GSoft.Cqrs.Abstractions.Events;

using Microsoft.Extensions.DependencyInjection;

namespace GSoft.Cqrs.Handlers;

public class EventHandlerWrapper<TEvent> : IEventHandlerWrapper
    where TEvent : IEvent
{
    private readonly IServiceProvider _serviceProvider;

    public EventHandlerWrapper(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public Task HandleAsync(IEvent @event, Func<IEnumerable<Func<IEvent, CancellationToken, Task>>, IEvent, CancellationToken, Task> publish, CancellationToken cancellationToken)
    {
        var handlers = this._serviceProvider.GetServices<IEventHandler<TEvent>>()
            .Select(x => new Func<IEvent, CancellationToken, Task>((innerEvent, token) => x.HandleAsync((TEvent)innerEvent, token)));

        return publish(handlers, @event, cancellationToken);
    }
}
