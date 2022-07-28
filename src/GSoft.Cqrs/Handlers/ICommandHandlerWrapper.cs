namespace GSoft.Cqrs.Handlers;

internal interface ICommandHandlerWrapper<TResponse> : IHandlerWrapper
{
    public Task<TResponse> HandleAsync(ICommand<TResponse> command, CancellationToken cancellationToken);
}

internal interface ICommandHandlerWrapper : IHandlerWrapper
{
    public Task HandleAsync(ICommand command, CancellationToken cancellationToken);
}