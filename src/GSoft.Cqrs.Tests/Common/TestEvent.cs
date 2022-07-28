using GSoft.Cqrs.Abstractions.Events;

using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Tests.Common;

public record TestEvent(Guid SomeId) : IEvent;

public class TestEventsHandler : IEventHandler<TestEvent>
{
    private readonly ILogger<TestEventsHandler> _logger;

    public TestEventsHandler(ILogger<TestEventsHandler> logger)
    {
        this._logger = logger;
    }

    public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken)
    {
        this._logger.LogInformation("Handler {HandlerType} was call with {RequestId}", this.GetType(), @event.SomeId);

        return Task.CompletedTask;
    }
}

public class AnotherTestEventsHandler : IEventHandler<TestEvent>
{
    private readonly ILogger<AnotherTestEventsHandler> _logger;

    public AnotherTestEventsHandler(ILogger<AnotherTestEventsHandler> logger)
    {
        this._logger = logger;
    }

    public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken)
    {
        this._logger.LogInformation("Handler {HandlerType} was call with {RequestId}", this.GetType(), @event.SomeId);

        return Task.CompletedTask;
    }
}
