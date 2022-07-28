using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Tests.Common;

public record TestQuery(Guid SomeId) : IQuery<bool>;

public class TestQueryHandler : IQueryHandler<TestQuery, bool>
{
    private readonly ILogger<TestQueryHandler> _logger;

    public TestQueryHandler(ILogger<TestQueryHandler> logger)
    {
        this._logger = logger;
    }

    public Task<bool> HandleAsync(TestQuery query, CancellationToken cancellationToken)
    {
        this._logger.LogInformation("Handler {HandlerType} was call with {RequestId}", this.GetType(), query.SomeId);

        return Task.FromResult(true);
    }
}
