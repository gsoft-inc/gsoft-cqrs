# GSoft.Cqrs

This library is the GSoft implementation of the CQRS (Command and Query Responsibility Segregation) pattern.

# Getting started

<!-- Description how to start using the library -->

## Installation

> :warning: This library has a private build. The code is available for reference only.

## Basic use

### For queries

Define a record and a handler for your query in the same file with the query name as the filename:

```CSharp
// File: WeatherQuery.cs
public record WeatherQuery : IQuery<IEnumerable<WeatherForecast>>
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [UsedImplicitly]
    public class Handler : IQueryHandler<WeatherQuery, IEnumerable<WeatherForecast>>
    {
        public async Task<IEnumerable<WeatherForecast>> HandleAsync(WeatherQuery query, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray());
        }
    }
}
```

> 💡 It's a good practice to define the handler class as a nested class inside the query record. This avoids redundancy in the handler name. If the handler is defines outside the query class it must be prefixed the query name to make it unique

In your service's `program.cs` add the mediator and the handler registrations:

```CSharp
// File: Program.cs
// ...
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Add registration for the mediator
builder.Services.AddMediator();

// Add the registration for you query
builder.Services.AddHandler<MyQuery.Handler>();

// ...
```

Create a WebApi controller `WeatherForecastController` with the following implementation:

```CSharp
// File: WeatherForecastController.cs
using GSoft.Cqrs;

// ...

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeatherForecastController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
    {
        var weatherForecasts = await this._mediator.HandleAsync(new WeatherQuery(), cancellationToken);
        return weatherForecasts;
    }
}
```

# Contributing

[Contributing](CONTRIBUTING.md)
