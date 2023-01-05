using Microsoft.AspNetCore.Mvc;
using Alza.LinkComposer.Links.Sample;
using Alza.LinkComposer.Interfaces;

namespace Alza.LinkComposer.AspNetCore.Sample.Controllers
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
        private readonly ILinkComposer _linkComposer;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ILinkComposer linkComposer)
        {
            _logger = logger;
            _linkComposer = linkComposer;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _linkComposer.Link<WeatherForecastControllerLink>(x => x.Get());

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}