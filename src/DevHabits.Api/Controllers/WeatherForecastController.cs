using System.Security.Cryptography;
using DevHabits.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevHabits.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Tags("WeatherForecast")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<WeatherForecastController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves weather forecast data
    /// </summary>
    /// <returns>A collection of weather forecasts</returns>
    /// <response code="200">Returns the weather forecast data</response>
    /// <remarks>
    /// This is a demo endpoint that returns sample weather data.
    /// </remarks>
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType<IEnumerable<WeatherForecast>>(StatusCodes.Status200OK)]
    public IEnumerable<WeatherForecast> Get()
    {

        _logger.LogInformation("Getting weather forecast");

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
            Summary = Summaries[RandomNumberGenerator.GetInt32(Summaries.Length)]
        })
        .ToArray();
    }
}
