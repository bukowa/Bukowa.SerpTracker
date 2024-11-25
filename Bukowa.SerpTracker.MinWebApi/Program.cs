using System.ComponentModel;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ICosTam>(new MyICosTam());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => { options.DefaultFonts = false; });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/project", (
        [Description("Get all projects")] ICosTam costam
    ) =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new Project("", DateTime.Now, DateTime.Today)).ToArray();
        return forecast;
    })
    .WithName("GetProject")
    .Produces<Project[]>(StatusCodes.Status202Accepted);

app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record Project(string Name, DateTime Start, DateTime End);


public interface ICosTam
{
};

public class MyICosTam : ICosTam
{
};