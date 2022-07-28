using Microsoft.Extensions.Logging;

namespace GSoft.Cqrs.Tests.Common;

public class QueryMiddlewareTest : BaseMiddleware<TestQuery, bool>
{
    public QueryMiddlewareTest(ILogger<QueryMiddlewareTest> logger)
        : base(logger)
    {
    }
}

public class QueryMiddlewareTest2 : BaseMiddleware<TestQuery, bool>
{
    public QueryMiddlewareTest2(ILogger<QueryMiddlewareTest2> logger)
        : base(logger)
    {
    }
}