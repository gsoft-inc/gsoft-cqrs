using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Tests.Common;

public record TestCommand(Guid SomeId) : ICommand<bool>;

public class TestCommandHandler : ICommandHandler<TestCommand, bool>
{
    private readonly ILogger<TestCommandHandler> _logger;

    public TestCommandHandler(ILogger<TestCommandHandler> logger)
    {
        this._logger = logger;
    }

    public Task<bool> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        this._logger.LogInformation("Handler {HandlerType} was call with {RequestId}", this.GetType(), command.SomeId);

        return Task.FromResult(true);
    }
}

public record TestReturnlessCommand(Guid SomeId) : ICommand;

public class TestReturnlessCommandHandler : ICommandHandler<TestReturnlessCommand>
{
    private readonly ILogger<TestReturnlessCommand> _logger;

    public TestReturnlessCommandHandler(ILogger<TestReturnlessCommand> logger)
    {
        this._logger = logger;
    }

    public Task HandleAsync(TestReturnlessCommand command, CancellationToken cancellationToken)
    {
        this._logger.LogInformation("Handler {HandlerType} was call with {RequestId}", this.GetType(), command.SomeId);

        return Task.FromResult(true);
    }
}
