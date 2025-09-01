# Error Handling Patterns

When implementing error handling for the DevHabits project, follow these comprehensive patterns:

## Global Exception Handler

```csharp
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DevHabits.Api.Middleware;

/// <summary>
/// Global exception handler for the application
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred");

        var problemDetails = CreateProblemDetails(httpContext, exception);
        
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = validationEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            
            ArgumentException argumentEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = argumentEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            
            UnauthorizedAccessException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Authentication is required to access this resource",
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
            },
            
            KeyNotFoundException notFoundEx => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = notFoundEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },
            
            InvalidOperationException invalidOpEx => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = invalidOpEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            },
            
            NotSupportedException notSupportedEx => new ProblemDetails
            {
                Status = StatusCodes.Status501NotImplemented,
                Title = "Not Implemented",
                Detail = notSupportedEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2"
            },
            
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };

        problemDetails.Instance = context.Request.Path;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return problemDetails;
    }
}
```

## Custom Exception Classes

```csharp
namespace DevHabits.Api.Exceptions;

/// <summary>
/// Base exception class for domain-specific exceptions
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public sealed class ResourceNotFoundException : DomainException
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public ResourceNotFoundException(string resourceType, string resourceId)
        : base($"{resourceType} with ID '{resourceId}' was not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public sealed class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when a resource already exists
/// </summary>
public sealed class ResourceAlreadyExistsException : DomainException
{
    public string ResourceType { get; }
    public string ConflictingProperty { get; }
    public string ConflictingValue { get; }

    public ResourceAlreadyExistsException(string resourceType, string conflictingProperty, string conflictingValue)
        : base($"{resourceType} with {conflictingProperty} '{conflictingValue}' already exists")
    {
        ResourceType = resourceType;
        ConflictingProperty = conflictingProperty;
        ConflictingValue = conflictingValue;
    }
}

/// <summary>
/// Exception thrown when an operation is not allowed on a resource
/// </summary>
public sealed class OperationNotAllowedException : DomainException
{
    public string Operation { get; }
    public string ResourceType { get; }
    public string Reason { get; }

    public OperationNotAllowedException(string operation, string resourceType, string reason)
        : base($"Operation '{operation}' is not allowed on {resourceType}: {reason}")
    {
        Operation = operation;
        ResourceType = resourceType;
        Reason = reason;
    }
}
```

## Result Pattern Implementation

```csharp
namespace DevHabits.Api.Common;

/// <summary>
/// Represents the result of an operation
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public sealed class Result<T>
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; init; }
    public string? Error { get; init; }
    public Exception? Exception { get; init; }

    private Result(bool isSuccess, T? value, string? error, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Exception = exception;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> Failure(Exception exception) => new(false, default, exception.Message, exception);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }

    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<string, Task<TResult>> onFailure)
    {
        return IsSuccess ? await onSuccess(Value!) : await onFailure(Error!);
    }
}

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; init; }
    public Exception? Exception { get; init; }

    private Result(bool isSuccess, string? error, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Exception = exception;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(Exception exception) => new(false, exception.Message, exception);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error!);
    }
}
```

## Service Error Handling Example

```csharp
using DevHabits.Api.Common;
using DevHabits.Api.Database;
using DevHabits.Api.DTOs.Habits;
using DevHabits.Api.Entities;
using DevHabits.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Services;

/// <summary>
/// Habit service with proper error handling
/// </summary>
public sealed class HabitService(ApplicationDbContext dbContext, ILogger<HabitService> logger)
{
    public async Task<Result<HabitDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving habit with ID: {HabitId}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                return Result<HabitDto>.Failure("Habit ID cannot be null or empty");
            }

            var habit = await dbContext.Habits
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(HabitQueries.ProjectToDto())
                .FirstOrDefaultAsync(cancellationToken);

            if (habit == null)
            {
                logger.LogWarning("Habit with ID {HabitId} not found", id);
                throw new ResourceNotFoundException("Habit", id);
            }

            logger.LogInformation("Successfully retrieved habit with ID: {HabitId}", id);
            return Result<HabitDto>.Success(habit);
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException)
        {
            logger.LogError(ex, "Error retrieving habit with ID: {HabitId}", id);
            return Result<HabitDto>.Failure(ex);
        }
    }

    public async Task<Result<HabitDto>> CreateAsync(CreateHabitDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Creating new habit: {HabitName}", createDto.Name);

            // Check for existing habit with same name
            var existingHabit = await dbContext.Habits
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Name == createDto.Name, cancellationToken);

            if (existingHabit != null)
            {
                throw new ResourceAlreadyExistsException("Habit", "name", createDto.Name);
            }

            var habit = createDto.ToEntity();
            
            dbContext.Habits.Add(habit);
            await dbContext.SaveChangesAsync(cancellationToken);

            var habitDto = habit.ToDto();
            logger.LogInformation("Successfully created habit with ID: {HabitId}", habitDto.Id);
            
            return Result<HabitDto>.Success(habitDto);
        }
        catch (Exception ex) when (ex is not ResourceAlreadyExistsException)
        {
            logger.LogError(ex, "Error creating habit: {HabitName}", createDto.Name);
            return Result<HabitDto>.Failure(ex);
        }
    }

    public async Task<Result<HabitDto>> UpdateAsync(string id, UpdateHabitDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Updating habit with ID: {HabitId}", id);

            var habit = await dbContext.Habits
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (habit == null)
            {
                throw new ResourceNotFoundException("Habit", id);
            }

            // Business rule: Cannot update archived habits
            if (habit.IsArchived)
            {
                throw new OperationNotAllowedException("Update", "Habit", "Habit is archived");
            }

            // Check for name conflicts (if name is being changed)
            if (habit.Name != updateDto.Name)
            {
                var existingHabit = await dbContext.Habits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.Name == updateDto.Name && h.Id != id, cancellationToken);

                if (existingHabit != null)
                {
                    throw new ResourceAlreadyExistsException("Habit", "name", updateDto.Name);
                }
            }

            habit.UpdateFromDto(updateDto);
            await dbContext.SaveChangesAsync(cancellationToken);

            var habitDto = habit.ToDto();
            logger.LogInformation("Successfully updated habit with ID: {HabitId}", id);
            
            return Result<HabitDto>.Success(habitDto);
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException and not OperationNotAllowedException and not ResourceAlreadyExistsException)
        {
            logger.LogError(ex, "Error updating habit with ID: {HabitId}", id);
            return Result<HabitDto>.Failure(ex);
        }
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Deleting habit with ID: {HabitId}", id);

            var habit = await dbContext.Habits
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (habit == null)
            {
                throw new ResourceNotFoundException("Habit", id);
            }

            // Business rule: Cannot delete completed habits
            if (habit.Status == HabitStatus.Completed)
            {
                throw new OperationNotAllowedException("Delete", "Habit", "Cannot delete completed habits");
            }

            dbContext.Habits.Remove(habit);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully deleted habit with ID: {HabitId}", id);
            return Result.Success();
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException and not OperationNotAllowedException)
        {
            logger.LogError(ex, "Error deleting habit with ID: {HabitId}", id);
            return Result.Failure(ex);
        }
    }
}
```

## Controller Error Handling

```csharp
using DevHabits.Api.DTOs.Habits;
using DevHabits.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevHabits.Api.Controllers;

[ApiController]
[Route("api/habits")]
[Tags("Habits")]
public sealed class HabitsController(HabitService habitService) : ControllerBase
{
    /// <summary>
    /// Retrieves a habit by ID
    /// </summary>
    /// <param name="id">The habit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The habit details</returns>
    /// <response code="200">Returns the habit</response>
    /// <response code="400">If the habit ID is invalid</response>
    /// <response code="404">If the habit is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType<HabitDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HabitDto>> GetHabitAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await habitService.GetByIdAsync(id, cancellationToken);
        
        return result.Match<ActionResult<HabitDto>>(
            onSuccess: habit => Ok(habit),
            onFailure: error => BadRequest(new ProblemDetails 
            { 
                Title = "Bad Request", 
                Detail = error,
                Status = StatusCodes.Status400BadRequest
            })
        );
    }

    /// <summary>
    /// Creates a new habit
    /// </summary>
    /// <param name="createHabitDto">The habit data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created habit</returns>
    /// <response code="201">Returns the newly created habit</response>
    /// <response code="400">If the habit data is invalid</response>
    /// <response code="409">If a habit with the same name already exists</response>
    [HttpPost]
    [ProducesResponseType<HabitDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HabitDto>> CreateHabitAsync(CreateHabitDto createHabitDto, CancellationToken cancellationToken = default)
    {
        var result = await habitService.CreateAsync(createHabitDto, cancellationToken);
        
        return result.Match<ActionResult<HabitDto>>(
            onSuccess: habit => CreatedAtAction(nameof(GetHabitAsync), new { id = habit.Id }, habit),
            onFailure: error => BadRequest(new ProblemDetails 
            { 
                Title = "Bad Request", 
                Detail = error,
                Status = StatusCodes.Status400BadRequest
            })
        );
    }
}
```

## Registration and Configuration

```csharp
// In Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    
    if (builder.Environment.IsProduction())
    {
        logging.AddApplicationInsights();
    }
});

var app = builder.Build();

// Enable exception handling
app.UseExceptionHandler();
```

## Best Practices

1. **Global Exception Handler**: Use IExceptionHandler for consistent error responses
2. **Custom Exceptions**: Create domain-specific exception classes
3. **Result Pattern**: Use Result<T> for operation results instead of throwing exceptions
4. **Logging**: Log errors with appropriate levels and context
5. **Error Codes**: Include error codes for programmatic error handling
6. **ProblemDetails**: Use RFC 7807 Problem Details standard
7. **Don't Expose Internals**: Never expose sensitive information in error messages
8. **Validation**: Validate inputs early and provide clear error messages
9. **Correlation IDs**: Include trace/correlation IDs for debugging
10. **Monitor Errors**: Use Application Insights or similar for error tracking
