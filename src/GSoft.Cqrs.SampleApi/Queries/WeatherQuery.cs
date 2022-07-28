using JetBrains.Annotations;

namespace GSoft.Cqrs.SampleApi.Queries;

public record WeatherQuery : IQuery<IEnumerable<WeatherForecast>>
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
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
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                })
                .ToArray());
        }
    }
}