
using MinDiator.Interfaces;

public partial class GetWeatherForecast
{

    public record Query(DateOnly Date, int TemperatureC, string? Summary) : IRequest<IEnumerable<WeatherForecast>>;

    public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary) : IRequest<IEnumerable<WeatherForecast>>
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
