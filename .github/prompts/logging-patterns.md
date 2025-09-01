# Logging Patterns

When implementing logging for the DevHabits project, follow these structured logging patterns:

## Structured Logging Configuration

```csharp
// In Program.cs
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", "DevHabits.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithCorrelationId()
        .Enrich.WithSpan()
        .WriteTo.Console(outputTemplate: 
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/devhabits-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate: 
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.ApplicationInsights(
            builder.Configuration.GetConnectionString("ApplicationInsights"),
            TelemetryConverter.Traces)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information);
});

var app = builder.Build();

// Add request logging middleware
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error
            : LogEventLevel.Information;
    
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
        }
    };
});
```

## appsettings.json Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.ApplicationInsights"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/devhabits-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithCorrelationId",
      "WithSpan"
    ],
    "Properties": {
      "ApplicationName": "DevHabits.Api"
    }
  }
}
```

## Service Logging Patterns

```csharp
using Microsoft.Extensions.Logging;
using DevHabits.Api.Database;
using DevHabits.Api.DTOs.Habits;
using DevHabits.Api.Entities;

namespace DevHabits.Api.Services;

/// <summary>
/// Habit service with comprehensive logging
/// </summary>
public sealed class HabitService(ApplicationDbContext dbContext, ILogger<HabitService> logger)
{
    public async Task<HabitDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["HabitId"] = id,
            ["Operation"] = nameof(GetByIdAsync)
        });

        logger.LogInformation("Starting habit retrieval for ID: {HabitId}", id);

        try
        {
            var habit = await dbContext.Habits
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(HabitQueries.ProjectToDto())
                .FirstOrDefaultAsync(cancellationToken);

            if (habit == null)
            {
                logger.LogWarning("Habit not found for ID: {HabitId}", id);
                return null;
            }

            logger.LogInformation("Successfully retrieved habit: {HabitName} (ID: {HabitId})", 
                habit.Name, habit.Id);

            return habit;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving habit with ID: {HabitId}", id);
            throw;
        }
    }

    public async Task<HabitDto> CreateAsync(CreateHabitDto createDto, CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["HabitName"] = createDto.Name,
            ["HabitType"] = createDto.Type.ToString(),
            ["Operation"] = nameof(CreateAsync)
        });

        logger.LogInformation("Creating new habit: {HabitName} of type {HabitType}", 
            createDto.Name, createDto.Type);

        try
        {
            // Log validation checks
            logger.LogDebug("Validating habit creation data for: {HabitName}", createDto.Name);

            var habit = createDto.ToEntity();
            
            logger.LogDebug("Generated habit ID: {HabitId}", habit.Id);

            dbContext.Habits.Add(habit);
            await dbContext.SaveChangesAsync(cancellationToken);

            var habitDto = habit.ToDto();

            logger.LogInformation("Successfully created habit: {HabitName} with ID: {HabitId}", 
                habitDto.Name, habitDto.Id);

            // Log habit details for audit
            logger.LogDebug("Habit details: {@HabitDetails}", new
            {
                habitDto.Id,
                habitDto.Name,
                habitDto.Type,
                habitDto.Status,
                Frequency = habitDto.Frequency?.Type,
                FrequencyTimes = habitDto.Frequency?.TimesPerPeriod,
                Target = habitDto.Target?.Value,
                TargetUnit = habitDto.Target?.Unit
            });

            return habitDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create habit: {HabitName}", createDto.Name);
            throw;
        }
    }

    public async Task<HabitDto> UpdateAsync(string id, UpdateHabitDto updateDto, CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["HabitId"] = id,
            ["Operation"] = nameof(UpdateAsync)
        });

        logger.LogInformation("Updating habit with ID: {HabitId}", id);

        try
        {
            var habit = await dbContext.Habits
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (habit == null)
            {
                logger.LogWarning("Attempt to update non-existent habit: {HabitId}", id);
                throw new KeyNotFoundException($"Habit with ID {id} not found");
            }

            // Log what's being updated
            var changes = new List<string>();
            if (habit.Name != updateDto.Name) changes.Add($"Name: {habit.Name} -> {updateDto.Name}");
            if (habit.Description != updateDto.Description) changes.Add($"Description changed");
            
            if (changes.Count > 0)
            {
                logger.LogInformation("Updating habit {HabitId} - Changes: {Changes}", 
                    id, string.Join(", ", changes));
            }

            habit.UpdateFromDto(updateDto);
            await dbContext.SaveChangesAsync(cancellationToken);

            var habitDto = habit.ToDto();

            logger.LogInformation("Successfully updated habit: {HabitName} (ID: {HabitId})", 
                habitDto.Name, habitDto.Id);

            return habitDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update habit with ID: {HabitId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["HabitId"] = id,
            ["Operation"] = nameof(DeleteAsync)
        });

        logger.LogInformation("Deleting habit with ID: {HabitId}", id);

        try
        {
            var habit = await dbContext.Habits
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (habit == null)
            {
                logger.LogWarning("Attempt to delete non-existent habit: {HabitId}", id);
                return false;
            }

            logger.LogInformation("Removing habit: {HabitName} (ID: {HabitId})", habit.Name, id);

            dbContext.Habits.Remove(habit);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully deleted habit: {HabitName} (ID: {HabitId})", 
                habit.Name, id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete habit with ID: {HabitId}", id);
            throw;
        }
    }
}
```

## Controller Logging Patterns

```csharp
using Microsoft.AspNetCore.Mvc;
using DevHabits.Api.DTOs.Habits;
using DevHabits.Api.Services;

namespace DevHabits.Api.Controllers;

[ApiController]
[Route("api/habits")]
[Tags("Habits")]
public sealed class HabitsController(HabitService habitService, ILogger<HabitsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves all habits
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<HabitDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HabitDto>>> GetHabitsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["Action"] = nameof(GetHabitsAsync),
            ["Page"] = page,
            ["Limit"] = limit
        });

        logger.LogInformation("Retrieving habits - Page: {Page}, Limit: {Limit}", page, limit);

        var habits = await habitService.GetAllAsync(page, limit, cancellationToken);

        logger.LogInformation("Retrieved {HabitCount} habits", habits.Count());

        return Ok(habits);
    }

    /// <summary>
    /// Creates a new habit
    /// </summary>
    [HttpPost]
    [ProducesResponseType<HabitDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HabitDto>> CreateHabitAsync(
        CreateHabitDto createHabitDto,
        CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["Action"] = nameof(CreateHabitAsync),
            ["HabitName"] = createHabitDto.Name,
            ["HabitType"] = createHabitDto.Type.ToString()
        });

        logger.LogInformation("Creating habit: {HabitName}", createHabitDto.Name);

        try
        {
            var habit = await habitService.CreateAsync(createHabitDto, cancellationToken);

            logger.LogInformation("Habit created successfully with ID: {HabitId}", habit.Id);

            return CreatedAtAction(nameof(GetHabitAsync), new { id = habit.Id }, habit);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create habit: {HabitName}", createHabitDto.Name);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific habit by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType<HabitDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HabitDto>> GetHabitAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["Action"] = nameof(GetHabitAsync),
            ["HabitId"] = id
        });

        logger.LogDebug("Retrieving habit with ID: {HabitId}", id);

        var habit = await habitService.GetByIdAsync(id, cancellationToken);

        if (habit == null)
        {
            logger.LogInformation("Habit not found: {HabitId}", id);
            return NotFound();
        }

        logger.LogDebug("Successfully retrieved habit: {HabitName}", habit.Name);

        return Ok(habit);
    }
}
```

## Background Service Logging

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DevHabits.Api.Services;

/// <summary>
/// Background service for habit progress calculations
/// </summary>
public sealed class HabitProgressBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<HabitProgressBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Habit progress background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var activity = logger.BeginScope(new Dictionary<string, object>
                {
                    ["Operation"] = "ProgressCalculation",
                    ["Timestamp"] = DateTime.UtcNow
                });

                logger.LogInformation("Starting habit progress calculation cycle");

                using var scope = serviceProvider.CreateScope();
                var progressService = scope.ServiceProvider.GetRequiredService<HabitProgressService>();

                var processedCount = await progressService.UpdateAllProgressAsync(stoppingToken);

                logger.LogInformation("Completed habit progress calculation - Processed {ProcessedCount} habits", 
                    processedCount);

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Habit progress background service cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in habit progress background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        logger.LogInformation("Habit progress background service stopped");
    }
}
```

## Entity Framework Logging

```csharp
// In ApplicationDbContext
public sealed class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Log entities being saved
            var entries = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            if (entries.Count > 0)
            {
                _logger.LogDebug("Saving changes - {EntryCount} entities affected", entries.Count);

                foreach (var entry in entries)
                {
                    _logger.LogDebug("Entity {EntityType} - State: {EntityState}, Key: {EntityKey}",
                        entry.Entity.GetType().Name,
                        entry.State,
                        entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue);
                }
            }

            var result = await base.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                _logger.LogInformation("Successfully saved {ChangeCount} changes to database", result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }
}
```

## Performance Logging

```csharp
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DevHabits.Api.Extensions;

/// <summary>
/// Extension methods for performance logging
/// </summary>
public static class LoggerExtensions
{
    public static IDisposable BeginTimedOperation(this ILogger logger, string operationName, params object[] args)
    {
        return new TimedOperation(logger, operationName, args);
    }

    private sealed class TimedOperation : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly object[] _args;
        private readonly Stopwatch _stopwatch;

        public TimedOperation(ILogger logger, string operationName, object[] args)
        {
            _logger = logger;
            _operationName = operationName;
            _args = args;
            _stopwatch = Stopwatch.StartNew();
            
            _logger.LogDebug("Starting operation: {OperationName}", _operationName);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            
            var elapsed = _stopwatch.ElapsedMilliseconds;
            var logLevel = elapsed switch
            {
                > 5000 => LogLevel.Warning,
                > 1000 => LogLevel.Information,
                _ => LogLevel.Debug
            };

            _logger.Log(logLevel, "Completed operation: {OperationName} in {ElapsedMs}ms", 
                _operationName, elapsed);
        }
    }
}

// Usage example
public async Task<IEnumerable<HabitDto>> GetAllAsync(CancellationToken cancellationToken = default)
{
    using var timer = logger.BeginTimedOperation("GetAllHabits");
    
    return await dbContext.Habits
        .AsNoTracking()
        .Select(HabitQueries.ProjectToDto())
        .ToListAsync(cancellationToken);
}
```

## Health Check Logging

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DevHabits.Api.HealthChecks;

/// <summary>
/// Database health check with logging
/// </summary>
public sealed class DatabaseHealthCheck(ApplicationDbContext dbContext, ILogger<DatabaseHealthCheck> logger) 
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Performing database health check");

            await dbContext.Database.CanConnectAsync(cancellationToken);

            logger.LogDebug("Database health check passed");
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
```

## Middleware Logging

```csharp
namespace DevHabits.Api.Middleware;

/// <summary>
/// Correlation ID middleware with logging
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);
        
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        context.Response.Headers[CorrelationIdHeader] = correlationId;

        logger.LogDebug("Processing request with correlation ID: {CorrelationId}", correlationId);

        await next(context);

        logger.LogDebug("Completed request with correlation ID: {CorrelationId}", correlationId);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId!;
        }

        return Guid.NewGuid().ToString();
    }
}
```

## Best Practices

1. **Structured Logging**: Use structured logging with named parameters
2. **Log Levels**: Use appropriate log levels (Debug, Information, Warning, Error, Critical)
3. **Correlation IDs**: Include correlation IDs for request tracing
4. **Performance Logging**: Log slow operations and performance metrics
5. **Security**: Never log sensitive information (passwords, tokens, PII)
6. **Context**: Include relevant context in log messages
7. **Exceptions**: Log exceptions with full stack traces
8. **Async Operations**: Use appropriate scoping for async operations
9. **Filtering**: Configure log filtering to avoid noise
10. **Retention**: Configure log retention policies for disk space management
