namespace GSoft.Cqrs.Abstractions.Events;
public interface IEventHandler<in TEvent> : IHandler
    where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
