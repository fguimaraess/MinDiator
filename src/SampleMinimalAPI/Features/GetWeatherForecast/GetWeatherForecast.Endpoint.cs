
using Microsoft.AspNetCore.Mvc;
using MinDiator;

namespace SampleAPI.Features.GetWeatherForecast;
public static class WeatherEndpoints
{
    public static void MapGetWeatherRoute(this WebApplication app)
    {
        app.MapGet("/weatherforecast", async ([FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new GetWeatherForecast.Query(DateOnly.FromDateTime(DateTime.Now), 100, "Teste"));
            return Results.Ok(result);
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi();
    }
}
