# Entity Creation Prompt

When creating new entities for the DevHabits project, follow these guidelines:

## Entity Structure Template

```csharp
namespace DevHabits.Api.Entities;

/// <summary>
/// Represents a [entity description]
/// </summary>
public sealed class [EntityName]
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the [property description]
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description
    /// </summary>
    public string? Description { get; set; }

    // Add domain-specific properties here

    /// <summary>
    /// Gets or sets the creation timestamp in UTC
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp in UTC
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties (if applicable)
    /// <summary>
    /// Gets or sets the related entities
    /// </summary>
    public List<RelatedEntity> RelatedEntities { get; set; } = [];
}
```

## Entity Configuration Template

```csharp
namespace DevHabits.Api.Database.Configurations;

/// <summary>
/// Entity Framework configuration for [EntityName]
/// </summary>
public sealed class [EntityName]Configuration : IEntityTypeConfiguration<[EntityName]>
{
    public void Configure(EntityTypeBuilder<[EntityName]> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc);

        // Configure relationships
        // builder.HasMany(e => e.RelatedEntities)
        //     .WithOne(r => r.Parent)
        //     .HasForeignKey(r => r.ParentId);

        // Configure indexes
        builder.HasIndex(e => e.Name);
    }
}
```

## Best Practices

1. **Always include audit fields**: `CreatedAtUtc`, `UpdatedAtUtc`
2. **Use string IDs**: Generate GUIDs as strings
3. **Initialize collections**: Use `= []` syntax for navigation properties
4. **Nullable reference types**: Use `?` for optional properties
5. **Sealed classes**: Mark entities as `sealed` for performance
6. **XML documentation**: Add comprehensive documentation
7. **Validation**: Create corresponding FluentValidation validators
8. **DTOs**: Create request/response DTOs for the entity
9. **Mappings**: Create mapping extensions between entity and DTOs
10. **Tests**: Write unit tests for entity behavior

## Required Files to Create

When adding a new entity, create these files:

1. **Entity**: `src/DevHabits.Api/Entities/[EntityName].cs`
2. **Configuration**: `src/DevHabits.Api/Database/Configurations/[EntityName]Configuration.cs`
3. **DTOs**:
   - `src/DevHabits.Api/DTOs/[EntityName]s/[EntityName]Dto.cs`
   - `src/DevHabits.Api/DTOs/[EntityName]s/Create[EntityName]Dto.cs`
   - `src/DevHabits.Api/DTOs/[EntityName]s/Update[EntityName]Dto.cs`
4. **Validators**:
   - `src/DevHabits.Api/DTOs/[EntityName]s/Create[EntityName]DtoValidator.cs`
   - `src/DevHabits.Api/DTOs/[EntityName]s/Update[EntityName]DtoValidator.cs`
5. **Mappings**: `src/DevHabits.Api/DTOs/[EntityName]s/[EntityName]Mappings.cs`
6. **Queries**: `src/DevHabits.Api/DTOs/[EntityName]s/[EntityName]Queries.cs`
7. **Controller**: `src/DevHabits.Api/Controllers/[EntityName]sController.cs`
8. **Migration**: Run `dotnet ef migrations add Add_[EntityName]s`

## Example Usage

```csharp
// Create entity
var entity = new EntityName
{
    Id = Guid.NewGuid().ToString(),
    Name = "Example",
    CreatedAtUtc = DateTime.UtcNow
};

// Update entity
entity.UpdatedAtUtc = DateTime.UtcNow;
```
