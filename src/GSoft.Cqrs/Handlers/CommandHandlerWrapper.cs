using GSoft.Cqrs.Abstractions.Middlewares;

namespace GSoft.Cqrs.Handlers;

public class CommandHandlerWrapper<TCommandHandler, TCommand, TResponse> : ICommandHandlerWrapper<TResponse>
    where TCommand : ICommand<TResponse>
    where TCommandHandler : ICommandHandler<TCommand, TResponse>
{
    private readonly TCommandHandler _handler;
    private readonly IEnumerable<IRequestMiddleware<TCommand, TResponse>> _commandHandlerMiddlewares;

    public CommandHandlerWrapper(TCommandHandler handler, IEnumerable<IRequestMiddleware<TCommand, TResponse>> commandHandlerMiddlewares)
    {
        this._handler = handler;
        this._commandHandlerMiddlewares = commandHandlerMiddlewares.Reverse();
    }

    public Task<TResponse> HandleAsync(ICommand<TResponse> command, CancellationToken cancellationToken)
    {
        Task<TResponse> Handler() => this._handler.HandleAsync((TCommand)command, cancellationToken);
        var next = this._commandHandlerMiddlewares
            .Aggregate(
                (RequestHandlerDelegate<TResponse>)Handler,
                (next, middleware) => () => middleware.HandleAsync(next, (TCommand)command, cancellationToken));

        return next();
    }
}

public class CommandHandlerWrapper<TCommandHandler, TCommand> : ICommandHandlerWrapper
    where TCommand : ICommand
    where TCommandHandler : ICommandHandler<TCommand>
{
    private readonly TCommandHandler _handler;
    private readonly IEnumerable<IRequestMiddleware<TCommand>> _commandHandlerMiddlewares;

    public CommandHandlerWrapper(TCommandHandler handler, IEnumerable<IRequestMiddleware<TCommand>> commandHandlerMiddlewares)
    {
        this._handler = handler;
        this._commandHandlerMiddlewares = commandHandlerMiddlewares.Reverse();
    }

    public Task HandleAsync(ICommand command, CancellationToken cancellationToken)
    {
        Task Handler() => this._handler.HandleAsync((TCommand)command, cancellationToken);
        var next = this._commandHandlerMiddlewares
            .Aggregate(
                (RequestHandlerDelegate)Handler,
                (next, middleware) => () => middleware.HandleAsync(next, (TCommand)command, cancellationToken));

        return next();
    }
}