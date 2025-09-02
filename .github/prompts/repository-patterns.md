# Repository Patterns

When implementing repository patterns for the DevHabits project, follow these guidelines:

## Repository Interface Template

```csharp
using System.Linq.Expressions;

namespace DevHabits.Api.Repositories;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Retrieves all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities matching the predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered collection of entities</returns>
    Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found</returns>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves first entity matching predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>First matching entity</returns>
    Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added entity</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity
    /// </summary>
    /// <param name="entity">Entity to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes entities matching predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entity matches</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching predicate
    /// </summary>
    /// <param name="predicate">Optional filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching entities</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
```

## Generic Repository Implementation

```csharp
using System.Linq.Expressions;
using DevHabits.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public sealed class Repository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity> 
    where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null 
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }
}
```

## Specialized Repository Interface

```csharp
using DevHabits.Api.Entities;

namespace DevHabits.Api.Repositories;

/// <summary>
/// Specialized repository interface for Habit operations
/// </summary>
public interface IHabitRepository : IRepository<Habit>
{
    /// <summary>
    /// Gets habits with their tags
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Habits with loaded tags</returns>
    Task<IEnumerable<Habit>> GetHabitsWithTagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active habits (not archived)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active habits</returns>
    Task<IEnumerable<Habit>> GetActiveHabitsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets habits by status
    /// </summary>
    /// <param name="status">Habit status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Habits with specified status</returns>
    Task<IEnumerable<Habit>> GetHabitsByStatusAsync(HabitStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets habits by type
    /// </summary>
    /// <param name="type">Habit type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Habits with specified type</returns>
    Task<IEnumerable<Habit>> GetHabitsByTypeAsync(HabitType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets habits that are due today based on frequency
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Habits due today</returns>
    Task<IEnumerable<Habit>> GetHabitsDueTodayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets habits with milestones
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Habits that have milestones</returns>
    Task<IEnumerable<Habit>> GetHabitsWithMilestonesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches habits by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching habits</returns>
    Task<IEnumerable<Habit>> SearchHabitsAsync(string searchTerm, CancellationToken cancellationToken = default);
}
```

## Specialized Repository Implementation

```csharp
using DevHabits.Api.Database;
using DevHabits.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Repositories;

/// <summary>
/// Specialized repository implementation for Habit operations
/// </summary>
public sealed class HabitRepository(ApplicationDbContext dbContext) : Repository<Habit>(dbContext), IHabitRepository
{
    public async Task<IEnumerable<Habit>> GetHabitsWithTagsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .Include(h => h.Tags)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Habit>> GetActiveHabitsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .Where(h => !h.IsArchived && h.Status == HabitStatus.Ongoing)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Habit>> GetHabitsByStatusAsync(HabitStatus status, CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .Where(h => h.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Habit>> GetHabitsByTypeAsync(HabitType type, CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .Where(h => h.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Habit>> GetHabitsDueTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        return await dbContext.Habits
            .AsNoTracking()
            .Where(h => !h.IsArchived && h.Status == HabitStatus.Ongoing)
            .Where(h => h.Frequency.Type == FrequencyType.Daily || 
                       (h.LastCompletedAtUtc == null || DateOnly.FromDateTime(h.LastCompletedAtUtc.Value) < today))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Habit>> GetHabitsWithMilestonesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .Where(h => h.Milestone != null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Habit>> SearchHabitsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();
        
        return await dbContext.Habits
            .AsNoTracking()
            .Where(h => h.Name.ToLower().Contains(normalizedSearchTerm) || 
                       (h.Description != null && h.Description.ToLower().Contains(normalizedSearchTerm)))
            .ToListAsync(cancellationToken);
    }
}
```

## Unit of Work Pattern

```csharp
using DevHabits.Api.Database;

namespace DevHabits.Api.Repositories;

/// <summary>
/// Unit of Work interface for managing multiple repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IHabitRepository Habits { get; }
    IRepository<Tag> Tags { get; }
    IRepository<HabitTag> HabitTags { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work implementation
/// </summary>
public sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private IHabitRepository? _habits;
    private IRepository<Tag>? _tags;
    private IRepository<HabitTag>? _habitTags;

    public IHabitRepository Habits => _habits ??= new HabitRepository(dbContext);
    public IRepository<Tag> Tags => _tags ??= new Repository<Tag>(dbContext);
    public IRepository<HabitTag> HabitTags => _habitTags ??= new Repository<HabitTag>(dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }
}
```

## Repository Registration

```csharp
// In Program.cs
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## Best Practices

1. **Generic Repositories**: Use generic repositories for common operations
2. **Specialized Repositories**: Create specialized repositories for complex queries
3. **Unit of Work**: Use Unit of Work pattern for transaction management
4. **Async Operations**: All repository methods should be async
5. **No Tracking**: Use AsNoTracking() for read-only operations
6. **Expression Trees**: Use Expression<Func<T, bool>> for flexible filtering
7. **Cancellation Support**: Support cancellation tokens
8. **Error Handling**: Let higher layers handle exceptions
9. **Separation of Concerns**: Keep repository focused on data access
10. **Performance**: Optimize queries and use appropriate loading strategies
