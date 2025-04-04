
using MinDiator.Interfaces;

namespace SampleAPI.Features.PostWeatherForecast;
public partial class PostWeatherForecast
{
    public record Command(DateOnly Date, int TemperatureC, string? Summary) : IRequest<IEnumerable<Response>>;
}
