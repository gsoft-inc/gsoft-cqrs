using GSoft.Cqrs.Abstractions.Middlewares;
using GSoft.Cqrs.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace GSoft.Cqrs.Tests.Middlewares;

public class MiddlewareTests : BaseServiceCollectionTest, IClassFixture<LoggingFixture>
{
    public MiddlewareTests(LoggingFixture fixture)
    {
        this.Services.AddMediator();
        this.Services.AddLogging(
            builder =>
            {
                builder.AddSerilog(fixture.GetLogger());
                builder.SetMinimumLevel(LogLevel.Trace);
            });
        this.Services.AddHandler<TestQueryHandler>();
        this.Services.AddHandler<TestCommandHandler>();
        this.Services.AddHandler<TestReturnlessCommandHandler>();
    }

    public IMediator GetMediator() => this.Provider.GetRequiredService<IMediator>();

    [Fact]
    public async Task Mediator_Will_Call_Middleware()
    {
        this.Services.AddSingleton(typeof(IRequestMiddleware<TestQuery, bool>), typeof(QueryMiddlewareTest));
        var expectedGuid = Guid.NewGuid();

        using var context = TestCorrelator.CreateContext();

        var result = await this.GetMediator().HandleAsync(new TestQuery(expectedGuid), CancellationToken.None);

        var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

        Assert.True(result);
        Assert.Collection(
            logs,
            firstLog =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest>(firstLog, true);
            },
            secondLog =>
            {
                AssertHandlerCalled<TestQueryHandler>(secondLog, expectedGuid);
            },
            thirdLog =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest>(thirdLog, false);
            });
    }

    [Fact]
    public async Task Mediator_Will_Call_Only_Valid_Middleware_For_Request_Type()
    {
        this.Services.AddSingleton(typeof(IRequestMiddleware<TestQuery, bool>), typeof(QueryMiddlewareTest));
        this.Services.AddSingleton(typeof(IRequestMiddleware<TestCommand, bool>), typeof(TestCommandMiddleware));
        var expectedGuid = Guid.NewGuid();

        using var context = TestCorrelator.CreateContext();

        var result = await this.GetMediator().HandleAsync(new TestQuery(expectedGuid), CancellationToken.None);

        var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

        Assert.True(result);
        Assert.Collection(
            logs,
            firstLog =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest>(firstLog, true);
            },
            secondLog =>
            {
                AssertHandlerCalled<TestQueryHandler>(secondLog, expectedGuid);
            },
            thirdLog =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest>(thirdLog, false);
            });
    }

    [Fact]
    public async Task Mediator_Will_Call_IRequestMiddleware_For_Queries_And_Commands()
    {
        this.Services.AddSingleton(typeof(IRequestMiddleware<,>), typeof(GenericMiddleWare<,>));
        var expectedGuid = Guid.NewGuid();

        using (var context = TestCorrelator.CreateContext())
        {
            var queryResult = await this.GetMediator().HandleAsync(new TestQuery(expectedGuid), CancellationToken.None);

            var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

            Assert.True(queryResult);
            Assert.Collection(
                logs,
                firstLog =>
                {
                    AssertMiddlewareCalled<GenericMiddleWare<TestQuery, bool>>(firstLog, true);
                },
                secondLog =>
                {
                    AssertHandlerCalled<TestQueryHandler>(secondLog, expectedGuid);
                },
                thirdLog =>
                {
                    AssertMiddlewareCalled<GenericMiddleWare<TestQuery, bool>>(thirdLog, false);
                });
        }

        using (var context = TestCorrelator.CreateContext())
        {
            var commandResult = await this.GetMediator().DispatchAsync(new TestCommand(expectedGuid), CancellationToken.None);

            var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

            Assert.True(commandResult);
            Assert.Collection(
                logs,
                firstLog =>
                {
                    AssertMiddlewareCalled<GenericMiddleWare<TestCommand, bool>>(firstLog, true);
                },
                secondLog =>
                {
                    AssertHandlerCalled<TestCommandHandler>(secondLog, expectedGuid);
                },
                thirdLog =>
                {
                    AssertMiddlewareCalled<GenericMiddleWare<TestCommand, bool>>(thirdLog, false);
                });
        }
    }

    [Fact]
    public async Task Mediator_Will_Respect_Orders_Of_Middlewares()
    {
        this.Services.AddSingleton(typeof(IRequestMiddleware<,>), typeof(GenericMiddleWare<,>));
        this.Services.AddSingleton(typeof(IRequestMiddleware<TestQuery, bool>), typeof(QueryMiddlewareTest));
        this.Services.AddSingleton(typeof(IRequestMiddleware<TestQuery, bool>), typeof(QueryMiddlewareTest2));
        var expectedGuid = Guid.NewGuid();

        using var context = TestCorrelator.CreateContext();

        var result = await this.GetMediator().HandleAsync(new TestQuery(expectedGuid), CancellationToken.None);

        var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

        Assert.True(result);
        Assert.Collection(
            logs,
            firstLog =>
            {
                AssertMiddlewareCalled<GenericMiddleWare<TestQuery, bool>>(firstLog, true);
            },
            secondLog =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest>(secondLog, true);
            },
            thirdLog =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest2>(thirdLog, true);
            },
            forthLog =>
            {
                AssertHandlerCalled<TestQueryHandler>(forthLog, expectedGuid);
            },
            fifthLog =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest2>(fifthLog, false);
            },
            sixth =>
            {
                AssertMiddlewareCalled<QueryMiddlewareTest>(sixth, false);
            },
            seventh =>
            {
                AssertMiddlewareCalled<GenericMiddleWare<TestQuery, bool>>(seventh, false);
            });
    }

    [Fact]
    public async Task Mediator_Will_Call_Middleware_For_Returnless_Commands()
    {
        this.Services.AddSingleton(typeof(IRequestMiddleware<>), typeof(TestReturnlessCommandMiddleware<>));
        var expectedGuid = Guid.NewGuid();

        using var context = TestCorrelator.CreateContext();

        await this.GetMediator().DispatchAsync(new TestReturnlessCommand(expectedGuid), CancellationToken.None);
        await this.GetMediator().DispatchAsync(new TestCommand(expectedGuid), CancellationToken.None);

        var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

        Assert.Collection(
            logs,
            firstLog =>
            {
                AssertMiddlewareCalled<TestReturnlessCommandMiddleware<TestReturnlessCommand>>(firstLog, true);
            },
            secondLog =>
            {
                AssertHandlerCalled<TestReturnlessCommandHandler>(secondLog, expectedGuid);
            },
            thirdLog =>
            {
                AssertMiddlewareCalled<TestReturnlessCommandMiddleware<TestReturnlessCommand>>(thirdLog, false);
            },
            fourthLog =>
            {
                AssertHandlerCalled<TestCommandHandler>(fourthLog, expectedGuid);
            });
    }

    private static void AssertHandlerCalled<THandler>(LogEvent log, Guid expectedGuid)
    {
        Assert.Equal(LogEventLevel.Information, log.Level);
        Assert.Equal(new ScalarValue(typeof(THandler).FullName), log.Properties["HandlerType"]);
        Assert.Equal(new ScalarValue(expectedGuid), log.Properties["RequestId"]);
    }

    private static void AssertMiddlewareCalled<TMiddleware>(LogEvent log, bool preProcessing)
    {
        var subString = preProcessing ? "pre-processing" : "post-processing";
        Assert.Equal(LogEventLevel.Information, log.Level);
        Assert.Equal(new ScalarValue(typeof(TMiddleware).FullName), log.Properties["MiddlewareType"]);
        Assert.Contains(subString, log.MessageTemplate.Text);
    }
}
