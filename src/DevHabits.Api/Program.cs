using DevHabits.Api.Database;
using DevHabits.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver =
    new CamelCasePropertyNamesContractResolver())
.AddXmlSerializerFormatters();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "DevHabits API",
            Version = "v1.0.0",
            Description = "A comprehensive API for tracking and managing daily habits with progress monitoring, milestone tracking, and flexible frequency configuration.",
            Contact = new()
            {
                Name = "DevHabits API Support",
                Email = "support@devhabits.com"
            },
            License = new()
            {
                Name = "MIT License",
                Url = new("https://opensource.org/licenses/MIT")
            }
        };

        document.Servers =
        [
           new() { Url = "https://localhost:5002", Description = "Development server" },
           new() { Url = "https://localhost:5001", Description = "Development server (HTTP)" }
        ];

        // Add tags for better organization
        document.Tags =
        [
            new() { Name = "Habits", Description = "Operations related to habit management" },
            new() { Name = "WeatherForecast", Description = "Demo weather forecast operations" }
        ];

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
            .UseSnakeCaseNamingConvention());

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddNpgsql())
    .WithMetrics(metrics => metrics
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
});

WebApplication? app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Add Scalar UI
    app.MapScalarApiReference();

    //app.MapScalarApiReference(options =>
    //{
    //    options
    //        .WithTitle("DevHabits API Documentation")
    //        .WithTheme(ScalarTheme.BluePlanet)
    //        .WithPreferredScheme("https")
    //        .WithSearchHotKey("Ctrl+K")
    //        .WithModels(true)
    //        .WithDownloadButton(true);
    //});

    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
