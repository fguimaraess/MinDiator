
using MinDiator.Interfaces;

namespace SampleAPI.Features.GetWeatherForecast;
public partial class GetWeatherForecast
{
    public record Query(DateOnly Date, int TemperatureC, string? Summary) : IRequest<IEnumerable<Response>>;
}
