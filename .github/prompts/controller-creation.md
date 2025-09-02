# Controller Creation Prompt

When creating API controllers for the DevHabits project, follow these guidelines:

## Controller Template

```csharp
using DevHabits.Api.Database;
using DevHabits.Api.DTOs.[EntityName]s;
using DevHabits.Api.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Controllers;

/// <summary>
/// API controller for managing [entity description]
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("[EntityName]s")]
public sealed class [EntityName]sController(ApplicationDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Retrieves all [entity name]s
    /// </summary>
    /// <returns>A collection of [entity name]s</returns>
    /// <response code="200">Returns the collection of [entity name]s</response>
    [HttpGet]
    [ProducesResponseType<[EntityName]sCollectionDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<[EntityName]sCollectionDto>> Get[EntityName]sAsync()
    {
        List<[EntityName]Dto> entities = await dbContext
            .[EntityName]s
            .Select([EntityName]Queries.ProjectToDto())
            .ToListAsync();

        var collection = new [EntityName]sCollectionDto
        {
            Data = entities
        };

        return Ok(collection);
    }

    /// <summary>
    /// Retrieves a specific [entity name] by ID
    /// </summary>
    /// <param name="id">The unique identifier of the [entity name]</param>
    /// <returns>The [entity name] with the specified ID</returns>
    /// <response code="200">Returns the [entity name]</response>
    /// <response code="404">If the [entity name] is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType<[EntityName]Dto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<[EntityName]Dto>> Get[EntityName]ByIdAsync(string id)
    {
        [EntityName]Dto? entity = await dbContext
            .[EntityName]s
            .Where(e => e.Id == id)
            .Select([EntityName]Queries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            return NotFound();
        }

        return Ok(entity);
    }

    /// <summary>
    /// Creates a new [entity name]
    /// </summary>
    /// <param name="create[EntityName]Dto">The [entity name] data to create</param>
    /// <returns>The created [entity name]</returns>
    /// <response code="201">Returns the newly created [entity name]</response>
    /// <response code="400">If the [entity name] data is invalid</response>
    [HttpPost]
    [ProducesResponseType<[EntityName]Dto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<[EntityName]Dto>> Create[EntityName]Async(Create[EntityName]Dto create[EntityName]Dto)
    {
        [EntityName] entity = create[EntityName]Dto.ToEntity();

        dbContext.[EntityName]s.Add(entity);
        await dbContext.SaveChangesAsync();

        [EntityName]Dto entityDto = entity.ToDto();

        return CreatedAtAction(
            nameof(Get[EntityName]ByIdAsync), 
            new { id = entityDto.Id }, 
            entityDto);
    }

    /// <summary>
    /// Updates an existing [entity name]
    /// </summary>
    /// <param name="id">The unique identifier of the [entity name]</param>
    /// <param name="update[EntityName]Dto">The updated [entity name] data</param>
    /// <returns>No content</returns>
    /// <response code="204">If the [entity name] was updated successfully</response>
    /// <response code="400">If the [entity name] data is invalid</response>
    /// <response code="404">If the [entity name] is not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update[EntityName]Async(string id, Update[EntityName]Dto update[EntityName]Dto)
    {
        [EntityName]? entity = await dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null)
        {
            return NotFound();
        }

        entity.UpdateFromDto(update[EntityName]Dto);
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Partially updates an existing [entity name]
    /// </summary>
    /// <param name="id">The unique identifier of the [entity name]</param>
    /// <param name="patchDocument">The JSON patch document</param>
    /// <returns>No content</returns>
    /// <response code="204">If the [entity name] was updated successfully</response>
    /// <response code="400">If the patch data is invalid</response>
    /// <response code="404">If the [entity name] is not found</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Patch[EntityName]Async(string id, JsonPatchDocument<[EntityName]Dto> patchDocument)
    {
        [EntityName]? entity = await dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null)
        {
            return NotFound();
        }

        [EntityName]Dto entityDto = entity.ToDto();

        patchDocument.ApplyTo(entityDto, ModelState);

        if (!TryValidateModel(entityDto))
        {
            return ValidationProblem(ModelState);
        }

        entity.UpdateFromDto(entityDto);
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a [entity name]
    /// </summary>
    /// <param name="id">The unique identifier of the [entity name]</param>
    /// <returns>No content</returns>
    /// <response code="204">If the [entity name] was deleted successfully</response>
    /// <response code="404">If the [entity name] is not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete[EntityName]Async(string id)
    {
        [EntityName]? entity = await dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null)
        {
            return NotFound();
        }

        dbContext.[EntityName]s.Remove(entity);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
```

## Best Practices

1. **Async/Await**: Use async methods for all I/O operations
2. **Primary Constructor**: Use primary constructor for dependency injection
3. **Sealed Classes**: Mark controllers as sealed
4. **XML Documentation**: Add comprehensive documentation for OpenAPI
5. **Response Types**: Use `[ProducesResponseType]` attributes
6. **HTTP Status Codes**: Return appropriate status codes
7. **Validation**: Leverage model validation and return proper error responses
8. **Error Handling**: Check for null entities and return NotFound
9. **Audit Fields**: Update `UpdatedAtUtc` on modifications
10. **RESTful Design**: Follow REST conventions for endpoints

## Required Attributes

- `[ApiController]`: Enables automatic model validation
- `[Route("api/[controller]")]`: Sets the base route
- `[Produces("application/json")]`: Sets default content type
- `[Tags("[EntityName]s")]`: Groups endpoints in OpenAPI documentation
- `[HttpGet]`, `[HttpPost]`, etc.: HTTP method attributes
- `[ProducesResponseType]`: Documents response types for OpenAPI

## Error Handling Patterns

```csharp
// Not Found
if (entity is null)
{
    return NotFound();
}

// Validation Error
if (!TryValidateModel(dto))
{
    return ValidationProblem(ModelState);
}

// Business Rule Violation
if (!businessRule.IsValid)
{
    return BadRequest("Business rule violation message");
}
```

## Query Patterns

```csharp
// Simple query
var entities = await dbContext.Entities.ToListAsync();

// Filtered query
var entities = await dbContext.Entities
    .Where(e => e.IsActive)
    .ToListAsync();

// Projected query
var dtos = await dbContext.Entities
    .Select(EntityQueries.ProjectToDto())
    .ToListAsync();

// Include related data
var entities = await dbContext.Entities
    .Include(e => e.RelatedEntities)
    .ToListAsync();
```
