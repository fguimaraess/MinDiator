
using MinDiator.Interfaces;

namespace SampleAPI.Features.PostWeatherForecast;
public partial class PostWeatherForecast
{
    public record Response(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
