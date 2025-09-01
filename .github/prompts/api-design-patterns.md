# API Design Patterns

When designing REST APIs for the DevHabits project, follow these established patterns and conventions:

## RESTful Endpoint Design

### Standard Resource Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json", "application/xml", "text/plain")]
[Tags("[EntityName]s")]
public sealed class [EntityName]sController(ApplicationDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Retrieves all [entityname]s with optional filtering and pagination
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">The page size (default: 10, max: 100)</param>
    /// <param name="search">Optional search term</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>Collection of [entityname]s</returns>
    /// <response code="200">Returns the collection of [entityname]s</response>
    [HttpGet]
    [ProducesResponseType<[EntityName]sCollectionDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<[EntityName]sCollectionDto>> Get[EntityName]sAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] [EntityName]Status? status = null)
    {
        var query = dbContext.[EntityName]s.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Name.Contains(search) || e.Description.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        // Apply pagination
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new [EntityName]Dto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Status = e.Status,
                CreatedAtUtc = e.CreatedAtUtc,
                UpdatedAtUtc = e.UpdatedAtUtc
            })
            .ToListAsync();

        return Ok(new [EntityName]sCollectionDto
        {
            Data = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            HasNextPage = (pageNumber * pageSize) < totalCount,
            HasPreviousPage = pageNumber > 1
        });
    }

    /// <summary>
    /// Retrieves a specific [entityname] by ID
    /// </summary>
    /// <param name="id">The [entityname] ID</param>
    /// <returns>The [entityname] details</returns>
    /// <response code="200">Returns the [entityname]</response>
    /// <response code="404">If the [entityname] is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType<[EntityName]Dto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<[EntityName]Dto>> Get[EntityName]ByIdAsync(string id)
    {
        var entity = await dbContext.[EntityName]s
            .Where(e => e.Id == id)
            .Select(e => new [EntityName]Dto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Status = e.Status,
                CreatedAtUtc = e.CreatedAtUtc,
                UpdatedAtUtc = e.UpdatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return NotFound();
        }

        return Ok(entity);
    }

    /// <summary>
    /// Creates a new [entityname]
    /// </summary>
    /// <param name="create[EntityName]Dto">The [entityname] data to create</param>
    /// <returns>The created [entityname]</returns>
    /// <response code="201">Returns the newly created [entityname]</response>
    /// <response code="400">If the [entityname] data is invalid</response>
    [HttpPost]
    [ProducesResponseType<[EntityName]Dto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<[EntityName]Dto>> Create[EntityName]Async(Create[EntityName]Dto create[EntityName]Dto)
    {
        var entity = new [EntityName]
        {
            Id = Guid.NewGuid().ToString(),
            Name = create[EntityName]Dto.Name,
            Description = create[EntityName]Dto.Description,
            Status = create[EntityName]Dto.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.[EntityName]s.Add(entity);
        await dbContext.SaveChangesAsync();

        var dto = new [EntityName]Dto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Status = entity.Status,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

        return CreatedAtAction(nameof(Get[EntityName]ByIdAsync), new { id = entity.Id }, dto);
    }

    /// <summary>
    /// Updates an existing [entityname]
    /// </summary>
    /// <param name="id">The [entityname] ID</param>
    /// <param name="update[EntityName]Dto">The updated [entityname] data</param>
    /// <returns>No content</returns>
    /// <response code="204">If the [entityname] was updated successfully</response>
    /// <response code="400">If the [entityname] data is invalid</response>
    /// <response code="404">If the [entityname] is not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update[EntityName]Async(string id, Update[EntityName]Dto update[EntityName]Dto)
    {
        var entity = await dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = update[EntityName]Dto.Name;
        entity.Description = update[EntityName]Dto.Description;
        entity.Status = update[EntityName]Dto.Status;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a [entityname]
    /// </summary>
    /// <param name="id">The [entityname] ID</param>
    /// <returns>No content</returns>
    /// <response code="204">If the [entityname] was deleted successfully</response>
    /// <response code="404">If the [entityname] is not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete[EntityName]Async(string id)
    {
        var entity = await dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
        {
            return NotFound();
        }

        dbContext.[EntityName]s.Remove(entity);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
```

## Content Negotiation

### Multiple Response Formats

```csharp
[Produces("application/json", "application/xml", "text/plain")]
public sealed class [EntityName]sController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<[EntityName]sCollectionDto>> Get[EntityName]sAsync()
    {
        var data = await GetDataAsync();
        
        // Content negotiation handled automatically by ASP.NET Core
        return Ok(data);
    }
}
```

### Custom Formatters

```csharp
// In Program.cs
builder.Services.Configure<MvcOptions>(options =>
{
    options.OutputFormatters.Add(new PlainTextFormatter());
    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
});
```

## Response Patterns

### Collection Response

```csharp
public sealed record [EntityName]sCollectionDto
{
    public required List<[EntityName]Dto> Data { get; init; }
    public required int TotalCount { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }
}
```

### Error Response

```csharp
public sealed record ErrorDto
{
    public required string Title { get; init; }
    public required string Detail { get; init; }
    public required int Status { get; init; }
    public string? TraceId { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}
```

### Success Response

```csharp
public sealed record SuccessDto<T>
{
    public required T Data { get; init; }
    public required string Message { get; init; }
    public required bool Success { get; init; } = true;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
```

## Advanced Endpoint Patterns

### Nested Resources

```csharp
/// <summary>
/// Retrieves tags for a specific habit
/// </summary>
/// <param name="habitId">The habit ID</param>
/// <returns>Collection of tags for the habit</returns>
[HttpGet("{habitId}/tags")]
[ProducesResponseType<TagsCollectionDto>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<TagsCollectionDto>> GetHabitTagsAsync(string habitId)
{
    var habitExists = await dbContext.Habits.AnyAsync(h => h.Id == habitId);
    if (!habitExists)
    {
        return NotFound($"Habit with ID {habitId} not found");
    }

    var tags = await dbContext.HabitTags
        .Where(ht => ht.HabitId == habitId)
        .Select(ht => new TagDto
        {
            Id = ht.Tag.Id,
            Name = ht.Tag.Name,
            Color = ht.Tag.Color,
            CreatedAtUtc = ht.Tag.CreatedAtUtc
        })
        .ToListAsync();

    return Ok(new TagsCollectionDto { Data = tags, TotalCount = tags.Count });
}

/// <summary>
/// Adds a tag to a habit
/// </summary>
/// <param name="habitId">The habit ID</param>
/// <param name="tagId">The tag ID</param>
/// <returns>No content</returns>
[HttpPost("{habitId}/tags/{tagId}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<IActionResult> AddTagToHabitAsync(string habitId, string tagId)
{
    var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == habitId);
    if (habit == null)
    {
        return NotFound($"Habit with ID {habitId} not found");
    }

    var tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
    if (tag == null)
    {
        return NotFound($"Tag with ID {tagId} not found");
    }

    var existingRelation = await dbContext.HabitTags
        .AnyAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);

    if (existingRelation)
    {
        return Conflict("Tag is already associated with this habit");
    }

    var habitTag = new HabitTag
    {
        HabitId = habitId,
        TagId = tagId,
        CreatedAtUtc = DateTime.UtcNow
    };

    dbContext.HabitTags.Add(habitTag);
    await dbContext.SaveChangesAsync();

    return NoContent();
}
```

### Bulk Operations

```csharp
/// <summary>
/// Creates multiple [entityname]s in a single operation
/// </summary>
/// <param name="create[EntityName]sDtos">The collection of [entityname]s to create</param>
/// <returns>The created [entityname]s</returns>
[HttpPost("bulk")]
[ProducesResponseType<[EntityName]sCollectionDto>(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<[EntityName]sCollectionDto>> CreateBulk[EntityName]sAsync(
    List<Create[EntityName]Dto> create[EntityName]sDtos)
{
    if (create[EntityName]sDtos.Count > 100)
    {
        return BadRequest("Cannot create more than 100 items at once");
    }

    var entities = create[EntityName]sDtos.Select(dto => new [EntityName]
    {
        Id = Guid.NewGuid().ToString(),
        Name = dto.Name,
        Description = dto.Description,
        Status = dto.Status,
        CreatedAtUtc = DateTime.UtcNow
    }).ToList();

    dbContext.[EntityName]s.AddRange(entities);
    await dbContext.SaveChangesAsync();

    var dtos = entities.Select(e => new [EntityName]Dto
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Status = e.Status,
        CreatedAtUtc = e.CreatedAtUtc,
        UpdatedAtUtc = e.UpdatedAtUtc
    }).ToList();

    return CreatedAtAction(nameof(Get[EntityName]sAsync), new [EntityName]sCollectionDto 
    { 
        Data = dtos, 
        TotalCount = dtos.Count 
    });
}
```

### Search and Filtering

```csharp
/// <summary>
/// Searches [entityname]s with advanced filtering
/// </summary>
/// <param name="searchDto">The search criteria</param>
/// <returns>Filtered collection of [entityname]s</returns>
[HttpPost("search")]
[ProducesResponseType<[EntityName]sCollectionDto>(StatusCodes.Status200OK)]
public async Task<ActionResult<[EntityName]sCollectionDto>> Search[EntityName]sAsync(
    Search[EntityName]sDto searchDto)
{
    var query = dbContext.[EntityName]s.AsQueryable();

    // Apply text search
    if (!string.IsNullOrWhiteSpace(searchDto.Query))
    {
        query = query.Where(e => 
            e.Name.Contains(searchDto.Query) || 
            e.Description.Contains(searchDto.Query));
    }

    // Apply filters
    if (searchDto.Status.HasValue)
    {
        query = query.Where(e => e.Status == searchDto.Status.Value);
    }

    if (searchDto.CreatedAfter.HasValue)
    {
        query = query.Where(e => e.CreatedAtUtc >= searchDto.CreatedAfter.Value);
    }

    if (searchDto.CreatedBefore.HasValue)
    {
        query = query.Where(e => e.CreatedAtUtc <= searchDto.CreatedBefore.Value);
    }

    // Apply sorting
    query = searchDto.SortBy?.ToLower() switch
    {
        "name" => searchDto.SortDescending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
        "created" => searchDto.SortDescending ? query.OrderByDescending(e => e.CreatedAtUtc) : query.OrderBy(e => e.CreatedAtUtc),
        _ => query.OrderBy(e => e.Name)
    };

    // Apply pagination
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
        .Take(searchDto.PageSize)
        .Select(e => new [EntityName]Dto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            Status = e.Status,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc
        })
        .ToListAsync();

    return Ok(new [EntityName]sCollectionDto
    {
        Data = items,
        TotalCount = totalCount,
        PageNumber = searchDto.PageNumber,
        PageSize = searchDto.PageSize,
        HasNextPage = (searchDto.PageNumber * searchDto.PageSize) < totalCount,
        HasPreviousPage = searchDto.PageNumber > 1
    });
}
```

## Error Handling Patterns

### Global Exception Handler

```csharp
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation errors occurred."
            },
            NotFoundException notFoundEx => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = notFoundEx.Message
            },
            ConflictException conflictEx => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = conflictEx.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            }
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### Validation Error Response

```csharp
public sealed record ValidationErrorDto
{
    public required string Title { get; init; } = "Validation Error";
    public required int Status { get; init; } = 400;
    public required Dictionary<string, string[]> Errors { get; init; }
    public string? TraceId { get; init; }
}
```

## API Documentation Patterns

### OpenAPI Configuration

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "DevHabits API",
            Version = "v1",
            Description = "API for managing daily habits and tracking progress",
            Contact = new() { Name = "DevHabits Team", Email = "support@devhabits.com" },
            License = new() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
        };

        document.Servers = [new() { Url = "https://api.devhabits.com", Description = "Production" }];

        return Task.CompletedTask;
    });
});
```

### Response Documentation

```csharp
/// <summary>
/// Creates a new habit with the provided details
/// </summary>
/// <param name="createHabitDto">The habit data to create</param>
/// <returns>The created habit with generated ID</returns>
/// <response code="201">Returns the newly created habit</response>
/// <response code="400">If the habit data is invalid</response>
/// <response code="409">If a habit with the same name already exists</response>
[HttpPost]
[ProducesResponseType<HabitDto>(StatusCodes.Status201Created)]
[ProducesResponseType<ValidationErrorDto>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ErrorDto>(StatusCodes.Status409Conflict)]
public async Task<ActionResult<HabitDto>> CreateHabitAsync(CreateHabitDto createHabitDto)
```

## Best Practices

1. **Consistent Naming**: Use plural nouns for collections (`/habits`, `/tags`)
2. **HTTP Methods**: Use appropriate HTTP verbs (GET, POST, PUT, DELETE)
3. **Status Codes**: Return correct HTTP status codes
4. **Pagination**: Implement pagination for large collections
5. **Filtering**: Support filtering and sorting parameters
6. **Content Negotiation**: Support multiple response formats
7. **Error Handling**: Provide consistent error responses
8. **Documentation**: Use comprehensive XML documentation
9. **Versioning**: Plan for API versioning
10. **Security**: Implement authentication and authorization
