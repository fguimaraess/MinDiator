
using MinDiator.Interfaces;
using static GetWeatherForecast;

public static class WeatherEndpoints
{
    public static void MapWeatherRoutes(this WebApplication app)
    {
        app.MapGet("/weatherforecast", async (IMinDiator mediator) =>
        {
            var result = await mediator.Send(new Query(DateOnly.FromDateTime(DateTime.Now), 100, "Teste"));
            return Results.Ok(result);
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi();
    }
}
