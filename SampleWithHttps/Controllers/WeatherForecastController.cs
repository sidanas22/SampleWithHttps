using Microsoft.AspNetCore.Mvc;

namespace SampleWithHttps.Controllers
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

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        //Call the Backend project to get the new mock
        [HttpGet("new")]
        public async Task<IActionResult> GetNewMock()
        {
            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://backend:8081/");
            HttpResponseMessage response = await client.GetAsync("WeatherForecast/new");
            if (response.IsSuccessStatusCode)
            {
                var person = await response.Content.ReadAsStringAsync();
                return Ok(person);
            }
            return BadRequest();
        }
    }
}
