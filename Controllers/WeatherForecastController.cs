using System;
using Demo.Cache.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;

namespace Demo.Cache.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("bycountry")]
        public async Task<IResult?> GetWeacherForcastByCountry([FromServices] HybridCache hybridCache
            , string city, CancellationToken ct)
        {
            var key = $"weather-{city}";
            WeatherForecast? weather = await hybridCache.GetOrCreateAsync<WeatherForecast>(key, async token =>
            {
                var weatherForecast = new WeatherForecast
                { 
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                    DateTime = DateTime.Now,
                };
                return weatherForecast;
            }, cancellationToken: ct);
            return Results.Ok(weather);
        }


        [HttpGet("bycity")]
        public async Task<WeatherForecast?> GetWeacherForcastByCity([FromServices] IDistributedCache distributedCache
            , string city, CancellationToken ct)
        {
            var key = $"weather-{city}";
            WeatherForecast? weather = await distributedCache.GetOrCreateAsync<WeatherForecast>(key, async token =>
            {
                var weatherForecast = new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                    DateTime = DateTime.Now,
                };
                return weatherForecast;
            }, ct: ct);
            return weather;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get([FromServices] IMemoryCache memoryCache)
        {
            var forecasts = memoryCache.GetOrCreate("weather", factory =>
            {
                factory.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);

                var randomForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                    DateTime = DateTime.Now,
                })
                .ToArray();

                return randomForecast;
            });
            return forecasts;
        }
    }
}
