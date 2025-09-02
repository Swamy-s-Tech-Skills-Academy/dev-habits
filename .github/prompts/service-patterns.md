# Service Patterns

When creating services for the DevHabits project, follow these patterns:

## Service Interface Template

```csharp
namespace DevHabits.Api.Services;

/// <summary>
/// Service interface for [entity name] operations
/// </summary>
public interface I[EntityName]Service
{
    /// <summary>
    /// Retrieves all [entity name]s with optional filtering
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of [entity name]s</returns>
    Task<IEnumerable<[EntityName]Dto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a [entity name] by ID
    /// </summary>
    /// <param name="id">The [entity name] ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The [entity name] if found</returns>
    Task<[EntityName]Dto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new [entity name]
    /// </summary>
    /// <param name="createDto">The [entity name] data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created [entity name]</returns>
    Task<[EntityName]Dto> CreateAsync(Create[EntityName]Dto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing [entity name]
    /// </summary>
    /// <param name="id">The [entity name] ID</param>
    /// <param name="updateDto">The updated [entity name] data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateAsync(string id, Update[EntityName]Dto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a [entity name]
    /// </summary>
    /// <param name="id">The [entity name] ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
```

## Service Implementation Template

```csharp
using DevHabits.Api.Database;
using DevHabits.Api.DTOs.[EntityName]s;
using DevHabits.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Services;

/// <summary>
/// Service implementation for [entity name] operations
/// </summary>
public sealed class [EntityName]Service(ApplicationDbContext dbContext, ILogger<[EntityName]Service> logger) : I[EntityName]Service
{
    public async Task<IEnumerable<[EntityName]Dto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving all [entity name]s");

        var entities = await dbContext.[EntityName]s
            .AsNoTracking()
            .Select([EntityName]Queries.ProjectToDto())
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} [entity name]s", entities.Count);
        return entities;
    }

    public async Task<[EntityName]Dto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving [entity name] with ID: {Id}", id);

        var entity = await dbContext.[EntityName]s
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select([EntityName]Queries.ProjectToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            logger.LogWarning("[EntityName] with ID {Id} not found", id);
        }

        return entity;
    }

    public async Task<[EntityName]Dto> CreateAsync(Create[EntityName]Dto createDto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating new [entity name]: {Name}", createDto.Name);

        var entity = createDto.ToEntity();
        
        dbContext.[EntityName]s.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created [entity name] with ID: {Id}", entity.Id);
        return entity.ToDto();
    }

    public async Task<bool> UpdateAsync(string id, Update[EntityName]Dto updateDto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating [entity name] with ID: {Id}", id);

        var entity = await dbContext.[EntityName]s
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            logger.LogWarning("[EntityName] with ID {Id} not found for update", id);
            return false;
        }

        entity.UpdateFromDto(updateDto);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated [entity name] with ID: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting [entity name] with ID: {Id}", id);

        var entity = await dbContext.[EntityName]s
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            logger.LogWarning("[EntityName] with ID {Id} not found for deletion", id);
            return false;
        }

        dbContext.[EntityName]s.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted [entity name] with ID: {Id}", id);
        return true;
    }
}
```

## Business Logic Service Template

```csharp
using DevHabits.Api.Database;
using DevHabits.Api.DTOs.Habits;
using DevHabits.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Services;

/// <summary>
/// Service for habit progress tracking and analytics
/// </summary>
public sealed class HabitProgressService(ApplicationDbContext dbContext, ILogger<HabitProgressService> logger)
{
    /// <summary>
    /// Calculates progress statistics for a habit
    /// </summary>
    /// <param name="habitId">The habit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Progress statistics</returns>
    public async Task<HabitProgressDto?> CalculateProgressAsync(string habitId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Calculating progress for habit: {HabitId}", habitId);

        var habit = await dbContext.Habits
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == habitId, cancellationToken);

        if (habit == null)
        {
            logger.LogWarning("Habit with ID {HabitId} not found", habitId);
            return null;
        }

        // Calculate today's progress
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayProgress = CalculateTodayProgress(habit);

        // Calculate streak
        var streak = await CalculateStreakAsync(habitId, cancellationToken);

        // Calculate completion rate
        var completionRate = await CalculateCompletionRateAsync(habitId, cancellationToken);

        return new HabitProgressDto
        {
            HabitId = habitId,
            TodayProgress = todayProgress,
            CurrentStreak = streak,
            CompletionRate = completionRate,
            IsCompletedToday = todayProgress >= 100,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks a habit as completed for today
    /// </summary>
    /// <param name="habitId">The habit ID</param>
    /// <param name="value">Progress value (for measurable habits)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if marked successfully</returns>
    public async Task<bool> MarkAsCompletedAsync(string habitId, int? value = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Marking habit {HabitId} as completed", habitId);

        var habit = await dbContext.Habits
            .FirstOrDefaultAsync(h => h.Id == habitId, cancellationToken);

        if (habit == null)
        {
            logger.LogWarning("Habit with ID {HabitId} not found", habitId);
            return false;
        }

        // Business logic for marking completion
        if (habit.Type == HabitType.Binary)
        {
            habit.LastCompletedAtUtc = DateTime.UtcNow;
        }
        else if (habit.Type == HabitType.Measurable && value.HasValue)
        {
            // Update milestone progress if applicable
            if (habit.Milestone != null)
            {
                habit.Milestone.Current += value.Value;
                if (habit.Milestone.Current >= habit.Milestone.Target)
                {
                    logger.LogInformation("Milestone achieved for habit {HabitId}", habitId);
                }
            }
            habit.LastCompletedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Habit {HabitId} marked as completed", habitId);
        return true;
    }

    private static decimal CalculateTodayProgress(Habit habit)
    {
        if (habit.Type == HabitType.Binary)
        {
            return habit.LastCompletedAtUtc?.Date == DateTime.UtcNow.Date ? 100 : 0;
        }

        if (habit.Type == HabitType.Measurable && habit.Milestone != null)
        {
            return Math.Min(100, (habit.Milestone.Current / (decimal)habit.Milestone.Target) * 100);
        }

        return 0;
    }

    private async Task<int> CalculateStreakAsync(string habitId, CancellationToken cancellationToken)
    {
        // Implementation for streak calculation
        // This would involve checking completion history
        await Task.CompletedTask; // Placeholder
        return 0;
    }

    private async Task<decimal> CalculateCompletionRateAsync(string habitId, CancellationToken cancellationToken)
    {
        // Implementation for completion rate calculation
        // This would involve checking completion history over a period
        await Task.CompletedTask; // Placeholder
        return 0;
    }
}

/// <summary>
/// DTO for habit progress information
/// </summary>
public sealed record HabitProgressDto
{
    public required string HabitId { get; init; }
    public required decimal TodayProgress { get; init; }
    public required int CurrentStreak { get; init; }
    public required decimal CompletionRate { get; init; }
    public required bool IsCompletedToday { get; init; }
    public required DateTime LastUpdated { get; init; }
}
```

## Service Registration

```csharp
// In Program.cs
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<HabitProgressService>();
```

## Best Practices

1. **Dependency Injection**: Use constructor injection for dependencies
2. **Logging**: Include comprehensive logging for operations
3. **Async Operations**: Use async/await for all database operations
4. **Error Handling**: Handle exceptions appropriately
5. **Business Logic**: Keep business logic in services, not controllers
6. **Separation of Concerns**: Separate data access from business logic
7. **Performance**: Use AsNoTracking() for read-only operations
8. **Cancellation Tokens**: Support cancellation for long-running operations
9. **Validation**: Validate inputs before processing
10. **Return Types**: Use appropriate return types (DTOs, not entities)
