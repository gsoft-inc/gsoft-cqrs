using GSoft.Cqrs.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

using Xunit;

namespace GSoft.Cqrs.Tests.Events;
public class EventTests : BaseServiceCollectionTest, IClassFixture<LoggingFixture>
{
    private readonly Guid _expectedGuid = Guid.NewGuid();
    private readonly TestEvent _defaultEvent;

    public EventTests(LoggingFixture fixture)
    {
        this.Services.AddMediator();
        this.Services.AddLogging(
            builder =>
            {
                builder.AddSerilog(fixture.GetLogger());
                builder.SetMinimumLevel(LogLevel.Trace);
            });
        this._defaultEvent = new TestEvent(this._expectedGuid);
    }

    public IMediator GetMediator() => this.Provider.GetRequiredService<IMediator>();

    [Fact]
    public async Task Nothing_Will_Happen_When_No_Handler_Registered()
    {
        var newEvent = new TestEvent(Guid.NewGuid());

        await this.GetMediator().Publish(newEvent, CancellationToken.None);
    }

    [Fact]
    public async Task Handler_Will_Be_Called_When_Event_Published()
    {
        this.Services.AddHandler<TestEventsHandler, TestEvent>();
        using var context = TestCorrelator.CreateContext();

        await this.GetMediator().Publish(this._defaultEvent);

        var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

        Assert.Collection(
            logs,
            firstLog =>
            {
                AssertHandlerCalled<TestEventsHandler>(firstLog, this._expectedGuid);
            });
    }

    [Fact]
    public async Task Given_Multiple_Handlers_All_Will_Be_Called()
    {
        this.Services.AddHandler<TestEventsHandler, TestEvent>();
        this.Services.AddHandler<AnotherTestEventsHandler, TestEvent>();
        using var context = TestCorrelator.CreateContext();

        await this.GetMediator().Publish(this._defaultEvent, CancellationToken.None);

        var logs = TestCorrelator.GetLogEventsFromContextGuid(context.Guid);

        AssertHandlerWasCalledWithoutOrder(this._expectedGuid, logs, typeof(TestEventsHandler), typeof(AnotherTestEventsHandler));
    }

    private static void AssertHandlerCalled<THandler>(LogEvent log, Guid expectedGuid)
    {
        Assert.Equal(LogEventLevel.Information, log.Level);
        Assert.Equal(new ScalarValue(typeof(THandler).FullName), log.Properties["HandlerType"]);
        Assert.Equal(new ScalarValue(expectedGuid), log.Properties["RequestId"]);
    }

    private static void AssertHandlerWasCalledWithoutOrder(Guid expectedGuid, IEnumerable<LogEvent> logs, params Type[] handlerTypes)
    {
        Assert.Contains(
            logs,
            log =>
            {
                if (log.Properties.TryGetValue("HandlerType", out var handlerType))
                {
                    Assert.Contains(handlerTypes, t => handlerType.Equals(new ScalarValue(t.FullName)));
                    Assert.Equal(LogEventLevel.Information, log.Level);
                    Assert.Equal(new ScalarValue(expectedGuid), log.Properties["RequestId"]);
                    return true;
                }

                return false;
            });
    }
}
