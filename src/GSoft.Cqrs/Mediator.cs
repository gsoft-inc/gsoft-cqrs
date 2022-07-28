using GSoft.Cqrs.Abstractions.Events;
using GSoft.Cqrs.Handlers;
using GSoft.Cqrs.Registrations;

using Microsoft.Extensions.DependencyInjection;

namespace GSoft.Cqrs;

internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RegistrationCollection _registrationCollection;

    public Mediator(IServiceProvider serviceProvider, RegistrationCollection registrationCollection)
    {
        this._serviceProvider = serviceProvider;
        this._registrationCollection = registrationCollection;
    }

    public Task<TResponse> HandleAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = this._registrationCollection.QueryRegistrations[query.GetType()];

        var handlerWrapper = (IQueryHandlerWrapper<TResponse>)this._serviceProvider.GetRequiredService(handlerType.WrapperType);

        return handlerWrapper.HandleAsync(query, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> StreamAsync<TResponse>(IStreamQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = this._registrationCollection.QueryStreamRegistrations[query.GetType()];

        var handlerWrapper = (IStreamHandlerWrapper<TResponse>)this._serviceProvider.GetRequiredService(handlerType.WrapperType);

        return handlerWrapper.StreamAsync(query, cancellationToken);
    }

    public Task<TResponse> DispatchAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        if (command == null)
        {
            throw new ArgumentException(null, nameof(command));
        }

        var handlerType = this._registrationCollection.CommandRegistrations[command.GetType()];

        var handlerWrapper = (ICommandHandlerWrapper<TResponse>)this._serviceProvider.GetRequiredService(handlerType.WrapperType);

        return handlerWrapper.HandleAsync(command, cancellationToken);
    }

    public Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerType = this._registrationCollection.CommandRegistrations[command.GetType()];

        var handlerWrapper = (ICommandHandlerWrapper)this._serviceProvider.GetRequiredService(handlerType.WrapperType);

        return handlerWrapper.HandleAsync(command, cancellationToken);
    }

    public Task Publish(IEvent @event, CancellationToken cancellationToken = default)
    {
        if (this._registrationCollection.EventRegistrations.TryGetValue(@event.GetType(), out var eventType))
        {
            var handlerWrapper = (IEventHandlerWrapper)this._serviceProvider.GetRequiredService(eventType.WrapperType);

            return handlerWrapper.HandleAsync(@event, PublishInternal, cancellationToken);
        }

        return Task.CompletedTask;
    }

    private static async Task PublishInternal(IEnumerable<Func<IEvent, CancellationToken, Task>> allHandlers, IEvent notification, CancellationToken cancellationToken)
    {
        foreach (var handler in allHandlers)
        {
            await handler(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}