# Performance Optimization Patterns

When optimizing performance for the DevHabits project, follow these patterns and best practices:

## Database Optimization Patterns

### Query Optimization

```csharp
using Microsoft.EntityFrameworkCore;
using DevHabits.Api.Database;
using DevHabits.Api.DTOs.Habits;
using System.Linq.Expressions;

namespace DevHabits.Api.Services;

/// <summary>
/// Optimized habit queries using projections and compiled queries
/// </summary>
public static class HabitQueries
{
    // Compiled query for frequently accessed habit by ID
    private static readonly Func<ApplicationDbContext, string, Task<HabitDto?>> GetHabitByIdCompiled =
        EF.CompileAsyncQuery((ApplicationDbContext context, string id) =>
            context.Habits
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(ProjectToDto())
                .FirstOrDefault());

    // Compiled query for paginated habits list
    private static readonly Func<ApplicationDbContext, int, int, IAsyncEnumerable<HabitDto>> GetHabitsPaginatedCompiled =
        EF.CompileAsyncQuery((ApplicationDbContext context, int skip, int take) =>
            context.Habits
                .AsNoTracking()
                .OrderBy(h => h.CreatedAtUtc)
                .Skip(skip)
                .Take(take)
                .Select(ProjectToDto()));

    // Compiled query for habit count
    private static readonly Func<ApplicationDbContext, Task<int>> GetHabitsCountCompiled =
        EF.CompileAsyncQuery((ApplicationDbContext context) =>
            context.Habits.Count());

    /// <summary>
    /// Projection expression for mapping Habit entity to HabitDto
    /// </summary>
    public static Expression<Func<Habit, HabitDto>> ProjectToDto()
    {
        return h => new HabitDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Type = h.Type,
            Status = h.Status,
            IsArchived = h.IsArchived,
            Frequency = h.Frequency != null ? new FrequencyDto
            {
                Type = h.Frequency.Type,
                TimesPerPeriod = h.Frequency.TimesPerPeriod
            } : null,
            Target = h.Target != null ? new TargetDto
            {
                Value = h.Target.Value,
                Unit = h.Target.Unit
            } : null,
            Milestone = h.Milestone != null ? new MilestoneDto
            {
                Target = h.Milestone.Target,
                Current = h.Milestone.Current,
                Unit = h.Milestone.Unit
            } : null,
            Tags = h.HabitTags.Select(ht => new TagDto
            {
                Id = ht.Tag.Id,
                Name = ht.Tag.Name,
                Color = ht.Tag.Color
            }).ToList(),
            CreatedAtUtc = h.CreatedAtUtc,
            UpdatedAtUtc = h.UpdatedAtUtc
        };
    }

    /// <summary>
    /// Get habit by ID using compiled query
    /// </summary>
    public static Task<HabitDto?> GetHabitByIdAsync(ApplicationDbContext context, string id)
    {
        return GetHabitByIdCompiled(context, id);
    }

    /// <summary>
    /// Get paginated habits using compiled query
    /// </summary>
    public static IAsyncEnumerable<HabitDto> GetHabitsPaginatedAsync(ApplicationDbContext context, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return GetHabitsPaginatedCompiled(context, skip, pageSize);
    }

    /// <summary>
    /// Get total habits count using compiled query
    /// </summary>
    public static Task<int> GetHabitsCountAsync(ApplicationDbContext context)
    {
        return GetHabitsCountCompiled(context);
    }
}

/// <summary>
/// Optimized habit service with performance considerations
/// </summary>
public sealed class OptimizedHabitService(ApplicationDbContext dbContext, IMemoryCache cache, ILogger<OptimizedHabitService> logger)
{
    private const int DefaultCacheExpirationMinutes = 5;

    public async Task<HabitDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"habit:{id}";
        if (cache.TryGetValue(cacheKey, out HabitDto? cachedHabit))
        {
            logger.LogDebug("Returning cached habit: {HabitId}", id);
            return cachedHabit;
        }

        // Use compiled query
        var habit = await HabitQueries.GetHabitByIdAsync(dbContext, id);

        if (habit != null)
        {
            // Cache for 5 minutes
            cache.Set(cacheKey, habit, TimeSpan.FromMinutes(DefaultCacheExpirationMinutes));
            logger.LogDebug("Cached habit: {HabitId}", id);
        }

        return habit;
    }

    public async Task<PagedResult<HabitDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Get total count using compiled query
        var totalCount = await HabitQueries.GetHabitsCountAsync(dbContext);

        // Get paginated data using compiled query
        var habits = await HabitQueries.GetHabitsPaginatedAsync(dbContext, page, pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<HabitDto>
        {
            Data = habits,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<IEnumerable<HabitDto>> GetActiveHabitsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "habits:active";
        
        if (cache.TryGetValue(cacheKey, out IEnumerable<HabitDto>? cachedHabits))
        {
            logger.LogDebug("Returning cached active habits");
            return cachedHabits!;
        }

        var habits = await dbContext.Habits
            .AsNoTracking()
            .Where(h => h.Status == HabitStatus.Ongoing && !h.IsArchived)
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync(cancellationToken);

        // Cache for 2 minutes (shorter since this changes frequently)
        cache.Set(cacheKey, habits, TimeSpan.FromMinutes(2));
        logger.LogDebug("Cached {HabitCount} active habits", habits.Count);

        return habits;
    }
}
```

### Bulk Operations

```csharp
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace DevHabits.Api.Services;

/// <summary>
/// Service for bulk operations on habits
/// </summary>
public sealed class HabitBulkService(ApplicationDbContext dbContext, ILogger<HabitBulkService> logger)
{
    public async Task<int> BulkUpdateStatusAsync(IEnumerable<string> habitIds, HabitStatus newStatus, CancellationToken cancellationToken = default)
    {
        using var timer = logger.BeginTimedOperation("BulkUpdateHabitStatus");

        var habits = await dbContext.Habits
            .Where(h => habitIds.Contains(h.Id))
            .ToListAsync(cancellationToken);

        foreach (var habit in habits)
        {
            habit.Status = newStatus;
            habit.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.BulkUpdateAsync(habits, cancellationToken);

        logger.LogInformation("Bulk updated {HabitCount} habits to status {Status}", 
            habits.Count, newStatus);

        return habits.Count;
    }

    public async Task<int> BulkArchiveHabitsAsync(IEnumerable<string> habitIds, CancellationToken cancellationToken = default)
    {
        using var timer = logger.BeginTimedOperation("BulkArchiveHabits");

        var affectedRows = await dbContext.Habits
            .Where(h => habitIds.Contains(h.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(h => h.IsArchived, true)
                .SetProperty(h => h.UpdatedAtUtc, DateTime.UtcNow), 
                cancellationToken);

        logger.LogInformation("Bulk archived {HabitCount} habits", affectedRows);

        return affectedRows;
    }

    public async Task BulkInsertHabitsAsync(IEnumerable<Habit> habits, CancellationToken cancellationToken = default)
    {
        using var timer = logger.BeginTimedOperation("BulkInsertHabits");

        await dbContext.BulkInsertAsync(habits.ToList(), cancellationToken);

        logger.LogInformation("Bulk inserted {HabitCount} habits", habits.Count());
    }
}
```

## Caching Strategies

### Memory Caching

```csharp
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DevHabits.Api.Services;

/// <summary>
/// Cache configuration options
/// </summary>
public sealed class CacheOptions
{
    public int DefaultExpirationMinutes { get; set; } = 5;
    public int ShortExpirationMinutes { get; set; } = 2;
    public int LongExpirationMinutes { get; set; } = 30;
    public int MaxCacheSize { get; set; } = 1000;
}

/// <summary>
/// Habit cache service with advanced caching strategies
/// </summary>
public sealed class HabitCacheService(IMemoryCache cache, IOptions<CacheOptions> options, ILogger<HabitCacheService> logger)
{
    private readonly CacheOptions _cacheOptions = options.Value;

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class
    {
        if (cache.TryGetValue(key, out T? cachedValue))
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return cachedValue;
        }

        logger.LogDebug("Cache miss for key: {CacheKey}", key);

        var value = await factory();
        
        if (value != null)
        {
            var cacheExpiration = expiration ?? TimeSpan.FromMinutes(_cacheOptions.DefaultExpirationMinutes);
            
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(_cacheOptions.DefaultExpirationMinutes / 2),
                Priority = CacheItemPriority.Normal,
                Size = 1
            };

            cache.Set(key, value, cacheEntryOptions);
            logger.LogDebug("Cached value for key: {CacheKey}, expires in {Expiration}", key, cacheExpiration);
        }

        return value;
    }

    public void InvalidateHabit(string habitId)
    {
        var keys = new[]
        {
            $"habit:{habitId}",
            "habits:active",
            "habits:completed",
            "habits:archived"
        };

        foreach (var key in keys)
        {
            cache.Remove(key);
            logger.LogDebug("Invalidated cache key: {CacheKey}", key);
        }
    }

    public void InvalidateTag(string tagId)
    {
        var keys = new[]
        {
            $"tag:{tagId}",
            "tags:all",
            "habits:active", // Habits include tags
            "habits:completed",
            "habits:archived"
        };

        foreach (var key in keys)
        {
            cache.Remove(key);
            logger.LogDebug("Invalidated cache key: {CacheKey}", key);
        }
    }
}
```

### Distributed Caching with Redis

```csharp
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DevHabits.Api.Services;

/// <summary>
/// Distributed cache service for Redis
/// </summary>
public sealed class DistributedCacheService(IDistributedCache distributedCache, ILogger<DistributedCacheService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cached = await distributedCache.GetStringAsync(key, cancellationToken);
            
            if (cached == null)
            {
                logger.LogDebug("Distributed cache miss for key: {CacheKey}", key);
                return null;
            }

            logger.LogDebug("Distributed cache hit for key: {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cached, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving from distributed cache: {CacheKey}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value, JsonOptions);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(expiration.TotalMinutes / 2)
            };

            await distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
            
            logger.LogDebug("Set distributed cache for key: {CacheKey}, expires in {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting distributed cache: {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
            logger.LogDebug("Removed from distributed cache: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing from distributed cache: {CacheKey}", key);
        }
    }
}
```

## Response Compression and Optimization

```csharp
// In Program.cs
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/xml",
        "text/plain"
    });
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

var app = builder.Build();

// Use response compression
app.UseResponseCompression();

// Add response caching headers
app.Use(async (context, next) =>
{
    // Cache static responses for 1 hour
    if (context.Request.Method == "GET" && 
        (context.Request.Path.StartsWithSegments("/api/habits") ||
         context.Request.Path.StartsWithSegments("/api/tags")))
    {
        context.Response.Headers.CacheControl = "public, max-age=3600";
        context.Response.Headers.ETag = $"\"{DateTime.UtcNow.Ticks}\"";
    }

    await next();
});
```

## Background Processing

```csharp
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace DevHabits.Api.Services;

/// <summary>
/// Background task item
/// </summary>
public sealed record BackgroundTaskItem(Func<CancellationToken, Task> Task, string Description);

/// <summary>
/// High-performance background task queue
/// </summary>
public sealed class BackgroundTaskQueue
{
    private readonly Channel<BackgroundTaskItem> _queue;
    private readonly ChannelWriter<BackgroundTaskItem> _writer;
    private readonly ChannelReader<BackgroundTaskItem> _reader;

    public BackgroundTaskQueue()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _queue = Channel.CreateBounded<BackgroundTaskItem>(options);
        _writer = _queue.Writer;
        _reader = _queue.Reader;
    }

    public async ValueTask QueueBackgroundWorkItemAsync(BackgroundTaskItem item)
    {
        await _writer.WriteAsync(item);
    }

    public IAsyncEnumerable<BackgroundTaskItem> DequeueAllAsync(CancellationToken cancellationToken)
    {
        return _reader.ReadAllAsync(cancellationToken);
    }
}

/// <summary>
/// Background task processor
/// </summary>
public sealed class BackgroundTaskProcessor(
    BackgroundTaskQueue taskQueue,
    ILogger<BackgroundTaskProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background task processor started");

        await foreach (var taskItem in taskQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                logger.LogDebug("Processing background task: {TaskDescription}", taskItem.Description);
                
                await taskItem.Task(stoppingToken);
                
                logger.LogDebug("Completed background task: {TaskDescription}", taskItem.Description);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing background task: {TaskDescription}", taskItem.Description);
            }
        }

        logger.LogInformation("Background task processor stopped");
    }
}
```

## API Response Optimization

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace DevHabits.Api.Controllers;

/// <summary>
/// Base controller with optimization features
/// </summary>
[ApiController]
public abstract class OptimizedControllerBase : ControllerBase
{
    private static readonly ConcurrentDictionary<Type, object> ResponseCache = new();

    protected ActionResult<T> CachedOk<T>(T value, string? etag = null)
    {
        if (etag != null)
        {
            Response.Headers.ETag = $"\"{etag}\"";
            
            if (Request.Headers.IfNoneMatch.Contains(etag))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
        }

        return Ok(value);
    }

    protected ActionResult<PagedResult<T>> PagedResult<T>(
        IEnumerable<T> data,
        int page,
        int pageSize,
        int totalCount)
    {
        var result = new PagedResult<T>
        {
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        // Add pagination headers
        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        Response.Headers.Add("X-Page", page.ToString());
        Response.Headers.Add("X-Page-Size", pageSize.ToString());
        Response.Headers.Add("X-Total-Pages", result.TotalPages.ToString());

        return Ok(result);
    }
}

/// <summary>
/// Optimized habits controller
/// </summary>
[Route("api/habits")]
[Tags("Habits")]
public sealed class OptimizedHabitsController(
    OptimizedHabitService habitService,
    ILogger<OptimizedHabitsController> logger) : OptimizedControllerBase
{
    /// <summary>
    /// Get habits with advanced filtering and pagination
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "page", "pageSize", "status", "type" })]
    public async Task<ActionResult<PagedResult<HabitDto>>> GetHabitsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] HabitStatus? status = null,
        [FromQuery] HabitType? type = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Min(100, Math.Max(1, pageSize)); // Limit to 100 items per page

        using var timer = logger.BeginTimedOperation("GetHabits");

        var result = await habitService.GetFilteredAsync(page, pageSize, status, type, search, cancellationToken);

        return PagedResult(result.Data, result.Page, result.PageSize, result.TotalCount);
    }

    /// <summary>
    /// Get habit by ID with conditional responses
    /// </summary>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 300, VaryByHeader = "If-None-Match")]
    public async Task<ActionResult<HabitDto>> GetHabitAsync(string id, CancellationToken cancellationToken = default)
    {
        var habit = await habitService.GetByIdAsync(id, cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        var etag = $"{habit.Id}-{habit.UpdatedAtUtc.Ticks}";
        return CachedOk(habit, etag);
    }
}
```

## Database Connection Optimization

```csharp
// In Program.cs
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Configure connection pooling
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
    
    // Optimize for production
    if (builder.Environment.IsProduction())
    {
        options.EnableSensitiveDataLogging(false);
        options.EnableDetailedErrors(false);
    }
    else
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}, poolSize: 128); // Pool size for high-concurrency scenarios

// Configure Npgsql connection pooling at driver level
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.ConfigureInternalBuilder(builder =>
{
    builder.Pooling = true;
    builder.MinPoolSize = 5;
    builder.MaxPoolSize = 100;
    builder.ConnectionLifetime = 300; // 5 minutes
    builder.ConnectionPruningInterval = 10;
});

using var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);
```

## Best Practices Summary

1. **Database Optimization**:
   - Use compiled queries for frequently executed queries
   - Implement proper indexing strategies
   - Use bulk operations for large data sets
   - Enable connection pooling

2. **Caching Strategies**:
   - Implement multi-level caching (memory + distributed)
   - Use appropriate cache invalidation strategies
   - Set proper expiration times
   - Monitor cache hit rates

3. **Response Optimization**:
   - Enable response compression (Gzip/Brotli)
   - Use ETags for conditional requests
   - Implement response caching
   - Optimize JSON serialization

4. **Background Processing**:
   - Use channels for high-performance queuing
   - Implement proper error handling
   - Monitor background task performance

5. **API Design**:
   - Implement pagination for large datasets
   - Use projections instead of full entity loading
   - Validate and limit request parameters
   - Add performance monitoring

6. **Monitoring**:
   - Log performance metrics
   - Monitor database query performance
   - Track cache hit/miss ratios
   - Set up alerts for slow operations
