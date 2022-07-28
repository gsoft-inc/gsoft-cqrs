using GSoft.Cqrs.SampleApi.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GSoft.Cqrs.SampleApi.Controllers;

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