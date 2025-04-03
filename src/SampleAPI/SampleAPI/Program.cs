using MinDiator.Extensions;
using MinDiator.Interfaces;
using SampleAPI.Behavior;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddMinDiator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
});

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
app.MapWeatherRoutes();

app.Run();
