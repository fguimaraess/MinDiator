namespace SampleAPI.Features.GetWeatherForecast;
public partial class GetWeatherForecast
{
    public record Response(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
