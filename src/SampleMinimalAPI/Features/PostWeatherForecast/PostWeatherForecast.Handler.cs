
using MinDiator.Interfaces;

namespace SampleAPI.Features.PostWeatherForecast;
public partial class PostWeatherForecast
{
    public class Handler : IRequestHandler<Command, IEnumerable<Response>>
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public async Task<IEnumerable<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new Response(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                )).ToArray();

            return await Task.FromResult(forecast.AsEnumerable());
        }
    }
}