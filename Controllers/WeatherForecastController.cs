using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

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

        [HttpGet("bycity")]
        public async Task<WeatherForecast> GetWeacherForcastByCity([FromServices] IDistributedCache distributedCache
            , string city, CancellationToken ct)
        {
            var checkCache = await distributedCache.GetStringAsync("weather", ct);
            if (string.IsNullOrEmpty(checkCache))
            {
                var weatherForecast = new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                    DateTime = DateTime.Now,
                };
                await distributedCache.SetStringAsync("weather", JsonConvert.SerializeObject(weatherForecast), ct);
                return weatherForecast;
            }
            return JsonConvert.DeserializeObject<WeatherForecast>(checkCache);
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
