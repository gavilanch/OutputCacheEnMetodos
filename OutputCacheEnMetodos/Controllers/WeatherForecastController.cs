using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Text;
using System.Text.Json;

namespace OutputCacheEnMetodos.Controllers
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
        private readonly IOutputCacheStore outputCacheStore;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IOutputCacheStore outputCacheStore)
        {
            _logger = logger;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            _logger.LogInformation("Utilizando el método get de WeatherForecast");

            var llave = "climas-obtener-todos";

            var valorDelCacheEnBytes = await outputCacheStore.GetAsync(llave, default);

            if (valorDelCacheEnBytes is not null)
            {
                // La data está en cache, podemos retornar
                var valorObtenidoDelCache =
                    ConvertirDeArregloDeBytesAObjeto<IEnumerable<WeatherForecast>>(valorDelCacheEnBytes);

                return valorObtenidoDelCache;
                
            }

            await Task.Delay(3000);

            var valor =  Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var valorEnArregloDeBytes = ConvertirObjetoAArregloDeBytes(valor);

            await outputCacheStore.SetAsync(llave, valorEnArregloDeBytes,
                tags: null, validFor: ValoresGlobales.TiempoDeExpiracionCache, default);

            return valor;
        }

        public static T ConvertirDeArregloDeBytesAObjeto<T>(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        public static byte[] ConvertirObjetoAArregloDeBytes(object obj)
        {
            string json = JsonSerializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
