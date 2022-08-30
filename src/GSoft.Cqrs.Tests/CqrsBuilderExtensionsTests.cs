using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace GSoft.Cqrs.Tests;

public class CqrsBuilderExtensionsTests : BaseServiceCollectionTest
{
    private readonly Type[] listOfHandlers = new[]
    {
        typeof(QueryHandler),
        typeof(QueryHandlerTwo),
        typeof(QueryHandlerWithInjection),
        typeof(SomeCommandHandler),
        typeof(SpyHandler),
        typeof(StreamHandlerTwo),
    };

    public CqrsBuilderExtensionsTests()
    {
        this.Services.AddMediator();
    }

    public IMediator GetMediator() => this.Provider.GetRequiredService<IMediator>();

    [Fact]
    public async Task AddHandler_Can_Register_Multiple_Queries()
    {
        this.Services.AddHandler<QueryHandler>();

        var mediator = this.GetMediator();
        var query1 = new QueryClassOne("res1");
        var query2 = new QueryRecordOne("res2");

        var res1 = await mediator.HandleAsync(query1);
        var res2 = await mediator.HandleAsync(query2);

        Assert.Equal("res1", res1);
        Assert.Equal("res2", res2);
    }

    [Fact]
    public void AddHandler_Multiple_Query_Registration_Throws_The_List_Of_Duplicated_Queries()
    {
        this.Services
            .AddHandler<QueryHandler>()
            .AddHandler<QueryHandlerTwo>();

        var exception = Assert.Throws<ArgumentException>(this.GetMediator);

        Assert.IsType<ArgumentException>(exception);
        Assert.Contains(nameof(QueryClassOne), exception.Message);
    }

    [Fact]
    public void AddHandler_Multiple_Query_Registration_Throws_The_List_Of_Duplicated_Streams()
    {
        this.Services
            .AddHandler<QueryHandler>()
            .AddHandler<StreamHandlerTwo>();

        var exception = Assert.Throws<ArgumentException>(this.GetMediator);

        Assert.IsType<ArgumentException>(exception);
        Assert.Contains(nameof(StreamOne), exception.Message);
    }

    [Fact]
    public async Task AddHandler_Same_Query_Can_Be_Stream_And_None_Stream()
    {
        this.Services.AddHandler<SpyHandler>();
        var mediator = this.GetMediator();
        var query = new SpyQuery("result");

        var regRes = await mediator.HandleAsync(query);
        var streamRes = await mediator.StreamAsync(query).ToArrayAsync();

        Assert.Equal("result", regRes);
        Assert.Equal("result", streamRes);
    }

    [Fact]
    public async Task AddHandler_Query_Can_Have_Parameters_Injected()
    {
        this.Services.AddHandler<QueryHandlerWithInjection>(p => ActivatorUtilities.CreateInstance<QueryHandlerWithInjection>(p, "Test"));
        var mediator = this.GetMediator();
        var query = new QueryRecordTwo("Result");

        var regRes = await mediator.HandleAsync(query);

        Assert.Equal("TestResult", regRes);
    }

    [Fact]
    public async Task AddHandler_Can_Register_Command_Handlers_And_Query_Handlers()
    {
        this.Services
            .AddHandler<QueryHandler>()
            .AddHandler<SomeCommandHandler>();

        var mediator = this.GetMediator();
        var query1 = new QueryClassOne("res1");
        var query2 = new QueryRecordOne("res2");
        var command = new SomeCommand("rabbles");
        var commandWithRes = new SomeCommandWithResult("rabbles");

        var res1 = await mediator.HandleAsync(query1);
        var res2 = await mediator.HandleAsync(query2);
        var res3 = await mediator.DispatchAsync(commandWithRes);
        await mediator.DispatchAsync(command);

        Assert.Equal("res1", res1);
        Assert.Equal("res2", res2);
        Assert.Equal("rabbles", res3);
    }

    [Fact]
    public void AddHandlers_Can_Register_Types_From_Assembly()
    {
        this.Services.AddHandlers(this.GetType().Assembly);

        this.Services.Select(s => s.ImplementationType).Should().Contain(this.listOfHandlers);
    }

    [Fact]
    public void AddHandlers_Can_Register_Filtered_List_Of_Types_From_Assembly()
    {
        this.Services.AddHandlers(t => t.Name.StartsWith("Query"), this.GetType().Assembly);

        this.Services.Select(s => s.ImplementationType).Should().Contain(this.listOfHandlers.Where(t => t.Name.StartsWith("Query")));
        this.Services.Select(s => s.ImplementationType).Should().NotContain(this.listOfHandlers.Where(t => !t.Name.StartsWith("Query")));
    }

    [Fact]
    public void AddMediator_Can_Register_Types_From_Assembly()
    {
        this.Services.AddMediator(this.GetType().Assembly);

        this.Services.Select(s => s.ImplementationType).Should().Contain(this.listOfHandlers);
    }

    private record struct QueryRecordStructOne(string Result) : IQuery<string>;

    private record struct SpyQuery(string Result) : IQuery<string>, IStreamQuery<char>;

    private class QueryClassOne : IQuery<string>
    {
        public QueryClassOne(string result)
        {
            this.Result = result;
        }

        public string Result { get; }
    }

    private record QueryRecordOne(string Result) : IQuery<string>;

    private record QueryRecordTwo(string Result) : IQuery<string>;

    private record StreamOne(string Result) : IStreamQuery<char>;

    private record SomeCommand(string Rabbles) : ICommand;

    private record SomeCommandWithResult(string Rabbles) : ICommand<string>;

    private class QueryHandler : IQueryHandler<QueryClassOne, string>,
                                 IQueryHandler<QueryRecordOne, string>,
                                 IQueryHandler<QueryRecordStructOne, string>,
                                 IStreamHandler<StreamOne, char>
    {
        public Task<string> HandleAsync(QueryClassOne query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Result);
        }

        public Task<string> HandleAsync(QueryRecordOne query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Result);
        }

        public Task<string> HandleAsync(QueryRecordStructOne query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Result);
        }

        public IAsyncEnumerable<char> StreamAsync(StreamOne query, CancellationToken cancellationToken)
        {
            return query.Result.ToAsyncEnumerable();
        }
    }

    private class SpyHandler : IQueryHandler<SpyQuery, string>, IStreamHandler<SpyQuery, char>
    {
        Task<string> IQueryHandler<SpyQuery, string>.HandleAsync(SpyQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Result);
        }

        IAsyncEnumerable<char> IStreamHandler<SpyQuery, char>.StreamAsync(SpyQuery query, CancellationToken cancellationToken)
        {
            return query.Result.ToAsyncEnumerable();
        }
    }

    private class QueryHandlerTwo : IQueryHandler<QueryClassOne, string>
    {
        public Task<string> HandleAsync(QueryClassOne query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Result);
        }
    }

    private class QueryHandlerWithInjection : IQueryHandler<QueryRecordTwo, string>
    {
        public QueryHandlerWithInjection(string injectedString)
        {
            this.InjectedString = injectedString;
        }

        public string InjectedString { get; set; }

        public Task<string> HandleAsync(QueryRecordTwo query, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.InjectedString + query.Result);
        }
    }

    private class StreamHandlerTwo : IStreamHandler<StreamOne, char>
    {
        public IAsyncEnumerable<char> StreamAsync(StreamOne query, CancellationToken cancellationToken)
        {
            return query.Result.ToAsyncEnumerable();
        }
    }

    private class SomeCommandHandler : ICommandHandler<SomeCommandWithResult, string>, ICommandHandler<SomeCommand>
    {
        public Task<string> HandleAsync(SomeCommandWithResult command, CancellationToken cancellationToken)
        {
            return Task.FromResult(command.Rabbles);
        }

        public Task HandleAsync(SomeCommand command, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}