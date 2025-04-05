
using Microsoft.AspNetCore.Mvc;
using MinDiator;

namespace SampleAPI.Features.PostWeatherForecast;
public static class WeatherEndpoints
{
    public static void MapPostWeatherRoute(this WebApplication app)
    {
        app.MapPost("/weatherforecast", async ([FromBody] Model command, [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new PostWeatherForecast.Command(command.Date.GetValueOrDefault(), command.Temp.GetValueOrDefault(), command.Summary));
            return Results.Ok(result);
        })
        .WithName("PostWeatherForecast")
        .WithOpenApi();
    }

    public class Model
    {
        public DateOnly? Date { get; set; }
        public int? Temp { get; set; }
        public string? Summary { get; set; }
    }
}
