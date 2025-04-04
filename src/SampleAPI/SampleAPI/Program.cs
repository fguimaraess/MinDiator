using MinDiator.Interfaces;
using SampleAPI.Behavior;
using SampleAPI.Features.GetWeatherForecast;
using SampleAPI.Features.PostWeatherForecast;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//MinDiator with explicit PipelineBehavior:
builder.Services.AddMinDiator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
});

//Simple MinDiator:
//builder.Services.AddMinDiator("SampleAPI");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type =>
    {
        // For nested types, use the full name with parent class
        if (type.DeclaringType != null)
        {
            return $"{type.DeclaringType.Name}.{type.Name}";
        }

        // For regular types, just use the type name
        return type.Name;
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGetWeatherRoute();
app.MapPostWeatherRoute();

app.Run();
