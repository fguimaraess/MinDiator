
using MinDiator.Interfaces;

public partial class GetWeatherForecast
{
    public class Handler : IRequestHandler<Query, IEnumerable<WeatherForecast>>
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public async Task<IEnumerable<WeatherForecast>> Handle(Query request, CancellationToken cancellationToken)
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                )).ToArray();

            return await Task.FromResult(forecast.AsEnumerable());
        }
    }
}