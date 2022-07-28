using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

using Xunit;

namespace GSoft.Cqrs.Tests;

public sealed class MediatorTests : BaseServiceCollectionTest, IClassFixture<LoggingFixture>
{
    public MediatorTests(LoggingFixture fixture)
    {
        this.Services.AddMediator();
        this.Services.AddLogging(
            builder =>
            {
                builder.AddSerilog(fixture.GetLogger());
                builder.SetMinimumLevel(LogLevel.Trace);
            });
    }

    public IMediator GetMediator() => this.Provider.GetRequiredService<IMediator>();

    [Fact]
    public void Mediator_Is_Registered_And_Transient()
    {
        var mediator = this.GetMediator();

        Assert.NotNull(mediator);
        Assert.NotSame(mediator, this.GetMediator());
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton, 1, 2, 3)]
    [InlineData(ServiceLifetime.Scoped, 1, 1, 1)]
    [InlineData(ServiceLifetime.Transient, 0, 0, 0)]
    public async Task Mediator_Respect_ServiceLifetime(ServiceLifetime lifetime, int scopeExpectedResult, int secondScopeExpectedResult, int finalExpectedResult)
    {
        this.Services.AddHandler<CounterHandler>(lifetime);
        var scopeFactory = this.Provider.GetRequiredService<IServiceScopeFactory>();
        var mediator = this.GetMediator();
        var initial = await mediator.HandleAsync(new GetNumberQuery());
        int scopeRes, secondScopeRes;

        using (var scope = scopeFactory.CreateScope())
        {
            var scopedMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await scopedMediator.DispatchAsync(new IncrementCommand());
            scopeRes = await scopedMediator.HandleAsync(new GetNumberQuery());
        }

        using (var scope = scopeFactory.CreateScope())
        {
            var scopedMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await scopedMediator.DispatchAsync(new IncrementCommand());
            secondScopeRes = await scopedMediator.HandleAsync(new GetNumberQuery());
        }

        await mediator.DispatchAsync(new IncrementCommand());
        var res = await mediator.HandleAsync(new GetNumberQuery());

        Assert.Equal(0, initial);
        Assert.Equal(scopeExpectedResult, scopeRes);
        Assert.Equal(secondScopeExpectedResult, secondScopeRes);
        Assert.Equal(finalExpectedResult, res);
    }

    private record GetResultQuery(int Res) : IQuery<int>;

    private class GetResultQueryHandler : IQueryHandler<GetResultQuery, int>
    {
        public Task<int> HandleAsync(GetResultQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Res);
        }
    }

    private record GetNumberQuery() : IQuery<int>;

    private record IncrementCommand() : ICommand;

    private class CounterHandler : IQueryHandler<GetNumberQuery, int>, ICommandHandler<IncrementCommand>
    {
        private int _cpt;

        public Task<int> HandleAsync(GetNumberQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(this._cpt);
        }

        public Task HandleAsync(IncrementCommand command, CancellationToken cancellationToken)
        {
            this._cpt++;

            return Task.CompletedTask;
        }
    }
}