using GSoft.Cqrs.Abstractions.Middlewares;

using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Tests.Common;

public class TestCommandMiddleware : BaseMiddleware<TestCommand, bool>
{
    protected TestCommandMiddleware(ILogger<BaseMiddleware<TestCommand, bool>> logger)
        : base(logger)
    {
    }
}

public class TestReturnlessCommandMiddleware<TCommand> : IRequestMiddleware<TCommand>
    where TCommand : ICommand
{
    public TestReturnlessCommandMiddleware(ILogger<TestReturnlessCommandMiddleware<TCommand>> logger)
    {
        this.Logger = logger;
    }

    protected ILogger<TestReturnlessCommandMiddleware<TCommand>> Logger { get; }

    public async Task HandleAsync(RequestHandlerDelegate next, TCommand request, CancellationToken cancellationToken)
    {
        this.Logger.LogInformation("Middleware {MiddlewareType} pre-processing", this.GetType().FullName);
        await next();
        this.Logger.LogInformation("Middleware {MiddlewareType} post-processing", this.GetType().FullName);
    }
}
