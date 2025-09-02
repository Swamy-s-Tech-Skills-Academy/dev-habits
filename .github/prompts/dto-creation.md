# DTO Creation Prompt

When creating Data Transfer Objects (DTOs) for the DevHabits project, follow these guidelines:

## DTO Structure Templates

### Request DTO (Create)

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Data transfer object for creating a new [entity name]
/// </summary>
public sealed record Create[EntityName]Dto
{
    /// <summary>
    /// Gets the name of the [entity name]
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional description of the [entity name]
    /// </summary>
    public string? Description { get; init; }

    // Add domain-specific properties with required modifier where appropriate
    /// <summary>
    /// Gets the [property description]
    /// </summary>
    public required PropertyType PropertyName { get; init; }

    /// <summary>
    /// Gets the optional [property description]
    /// </summary>
    public OptionalType? OptionalProperty { get; init; }
}
```

### Request DTO (Update)

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Data transfer object for updating an existing [entity name]
/// </summary>
public sealed record Update[EntityName]Dto
{
    /// <summary>
    /// Gets the name of the [entity name]
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional description of the [entity name]
    /// </summary>
    public string? Description { get; init; }

    // Include only updatable properties
    /// <summary>
    /// Gets the [property description]
    /// </summary>
    public required PropertyType PropertyName { get; init; }
}
```

### Response DTO

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Data transfer object representing a [entity name]
/// </summary>
public sealed record [EntityName]Dto
{
    /// <summary>
    /// Gets the unique identifier
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name of the [entity name]
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional description of the [entity name]
    /// </summary>
    public string? Description { get; init; }

    // Add all domain properties
    /// <summary>
    /// Gets the [property description]
    /// </summary>
    public required PropertyType PropertyName { get; init; }

    // Audit fields
    /// <summary>
    /// Gets the creation timestamp in UTC
    /// </summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets the last update timestamp in UTC
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }
}
```

### Collection DTO

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Data transfer object for a collection of [entity name]s
/// </summary>
public sealed record [EntityName]sCollectionDto : ICollectionResponse<[EntityName]Dto>
{
    /// <summary>
    /// Gets the collection of [entity name]s
    /// </summary>
    public required IReadOnlyList<[EntityName]Dto> Data { get; init; }
}
```

### Complex Type DTOs

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Data transfer object for [complex type description]
/// </summary>
public sealed record [ComplexType]Dto
{
    /// <summary>
    /// Gets the [property description]
    /// </summary>
    public required PropertyType PropertyName { get; init; }
}
```

## FluentValidation Validators

### Create DTO Validator

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Validator for Create[EntityName]Dto
/// </summary>
public sealed class Create[EntityName]DtoValidator : AbstractValidator<Create[EntityName]Dto>
{
    private static readonly string[] AllowedValues = ["value1", "value2", "value3"];

    public Create[EntityName]DtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Name must be between 3 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null)
            .WithMessage("Description cannot exceed 500 characters");

        // Custom validation rules
        RuleFor(x => x.PropertyName)
            .NotNull()
            .WithMessage("PropertyName is required");

        // Complex validation
        RuleFor(x => x)
            .Must(BeValidBusinessRule)
            .WithMessage("Business rule violation message");
    }

    private static bool BeValidBusinessRule(Create[EntityName]Dto dto)
    {
        // Implement business rule validation
        return true;
    }
}
```

### Update DTO Validator

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Validator for Update[EntityName]Dto
/// </summary>
public sealed class Update[EntityName]DtoValidator : AbstractValidator<Update[EntityName]Dto>
{
    public Update[EntityName]DtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        // Add update-specific validation rules
    }
}
```

## Mapping Extensions

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Mapping extensions for [EntityName] and related DTOs
/// </summary>
public static class [EntityName]Mappings
{
    /// <summary>
    /// Converts a Create[EntityName]Dto to a [EntityName] entity
    /// </summary>
    public static [EntityName] ToEntity(this Create[EntityName]Dto dto)
    {
        return new [EntityName]
        {
            Id = Guid.NewGuid().ToString(),
            Name = dto.Name,
            Description = dto.Description,
            PropertyName = dto.PropertyName,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Converts a [EntityName] entity to a [EntityName]Dto
    /// </summary>
    public static [EntityName]Dto ToDto(this [EntityName] entity)
    {
        return new [EntityName]Dto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            PropertyName = entity.PropertyName,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    /// <summary>
    /// Updates a [EntityName] entity from an Update[EntityName]Dto
    /// </summary>
    public static void UpdateFromDto(this [EntityName] entity, Update[EntityName]Dto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.PropertyName = dto.PropertyName;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates a [EntityName] entity from a [EntityName]Dto (for PATCH operations)
    /// </summary>
    public static void UpdateFromDto(this [EntityName] entity, [EntityName]Dto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.PropertyName = dto.PropertyName;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
```

## Query Projections

```csharp
namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Query projections for [EntityName]
/// </summary>
public static class [EntityName]Queries
{
    /// <summary>
    /// Projects a [EntityName] entity to a [EntityName]Dto
    /// </summary>
    public static Expression<Func<[EntityName], [EntityName]Dto>> ProjectToDto()
    {
        return entity => new [EntityName]Dto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            PropertyName = entity.PropertyName,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    /// <summary>
    /// Projects a [EntityName] entity to a [EntityName]Dto with related data
    /// </summary>
    public static Expression<Func<[EntityName], [EntityName]WithRelatedDto>> ProjectToWithRelatedDto()
    {
        return entity => new [EntityName]WithRelatedDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            PropertyName = entity.PropertyName,
            RelatedEntities = entity.RelatedEntities.Select(r => new RelatedEntityDto
            {
                Id = r.Id,
                Name = r.Name
            }).ToList(),
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
```

## Best Practices

1. **Record Types**: Use `record` for immutable DTOs
2. **Required Properties**: Use `required` modifier for mandatory properties
3. **Init-Only Properties**: Use `{ get; init; }` for immutability
4. **Nullable Reference Types**: Use `?` for optional properties
5. **XML Documentation**: Add comprehensive documentation
6. **Validation**: Create FluentValidation validators for request DTOs
7. **Mapping**: Create extension methods for entity-DTO conversions
8. **Query Projections**: Use LINQ expressions for efficient database queries
9. **Collection DTOs**: Implement `ICollectionResponse<T>` for collections
10. **Naming**: Follow consistent naming conventions

## File Organization

```
src/DevHabits.Api/DTOs/[EntityName]s/
├── [EntityName]Dto.cs
├── Create[EntityName]Dto.cs
├── Update[EntityName]Dto.cs
├── [EntityName]sCollectionDto.cs
├── [EntityName]WithRelatedDto.cs (if needed)
├── Create[EntityName]DtoValidator.cs
├── Update[EntityName]DtoValidator.cs
├── [EntityName]Mappings.cs
├── [EntityName]Queries.cs
└── [ComplexType]Dto.cs (if needed)
```
