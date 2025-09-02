# Migration Creation Prompt

When creating Entity Framework Core migrations for the DevHabits project, follow these guidelines:

## Migration Commands

### Creating Migrations

```bash
# Create a new migration
dotnet ef migrations add [MigrationName] --project src/DevHabits.Api

# Examples
dotnet ef migrations add InitialCreate --project src/DevHabits.Api
dotnet ef migrations add Add_HabitTags --project src/DevHabits.Api
dotnet ef migrations add Update_HabitFrequency --project src/DevHabits.Api
dotnet ef migrations add AddIndexes_Performance --project src/DevHabits.Api
```

### Applying Migrations

```bash
# Update database to latest migration
dotnet ef database update --project src/DevHabits.Api

# Update to specific migration
dotnet ef database update [MigrationName] --project src/DevHabits.Api

# Rollback to previous migration
dotnet ef database update [PreviousMigrationName] --project src/DevHabits.Api
```

### Migration Information

```bash
# List all migrations
dotnet ef migrations list --project src/DevHabits.Api

# View migration SQL
dotnet ef migrations script --project src/DevHabits.Api

# View SQL for specific migration
dotnet ef migrations script [FromMigration] [ToMigration] --project src/DevHabits.Api
```

## Migration Naming Conventions

### Schema Changes

- `Add_[EntityName]` - Adding new entities
- `Update_[EntityName][Property]` - Modifying existing entities
- `Remove_[EntityName][Property]` - Removing properties
- `Rename_[OldName]To[NewName]` - Renaming entities/properties

### Data Changes

- `Seed_[EntityName]Data` - Adding initial data
- `Migrate_[EntityName]Data` - Data transformation
- `Cleanup_[EntityName]Data` - Data cleanup

### Performance

- `AddIndexes_[EntityName]` - Adding indexes
- `OptimizeQueries_[EntityName]` - Query optimization

## Entity Configuration Template

```csharp
using DevHabits.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabits.Api.Database.Configurations;

/// <summary>
/// Entity Framework configuration for [EntityName]
/// </summary>
public sealed class [EntityName]Configuration : IEntityTypeConfiguration<[EntityName]>
{
    public void Configure(EntityTypeBuilder<[EntityName]> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasMaxLength(36)
            .IsRequired();

        // Required Properties
        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Audit Fields
        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.UpdatedAtUtc);

        // Owned Types (Value Objects)
        builder.OwnsOne(e => e.[OwnedProperty], owned =>
        {
            owned.Property(p => p.[Property1])
                .HasColumnName("[column_name]")
                .IsRequired();

            owned.Property(p => p.[Property2])
                .HasColumnName("[column_name2]");
        });

        // Relationships
        builder.HasMany(e => e.[NavigationProperty])
            .WithOne(n => n.[EntityName])
            .HasForeignKey(n => n.[EntityName]Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many Relationships
        builder.HasMany(e => e.[RelatedEntities])
            .WithMany(r => r.[EntityName]s)
            .UsingEntity<[JoinEntity]>(
                j => j
                    .HasOne(je => je.[RelatedEntity])
                    .WithMany(r => r.[JoinEntities])
                    .HasForeignKey(je => je.[RelatedEntity]Id),
                j => j
                    .HasOne(je => je.[EntityName])
                    .WithMany(e => e.[JoinEntities])
                    .HasForeignKey(je => je.[EntityName]Id),
                j =>
                {
                    j.HasKey(je => new { je.[EntityName]Id, je.[RelatedEntity]Id });
                    j.ToTable("[join_table_name]");
                });

        // Indexes
        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasIndex(e => e.CreatedAtUtc);

        builder.HasIndex(e => new { e.[Property1], e.[Property2] })
            .HasDatabaseName("ix_[entity_name]_[property1]_[property2]");

        // Table Name (PostgreSQL snake_case)
        builder.ToTable("[table_name]");
    }
}
```

## Complex Migration Examples

### Adding New Entity with Relationships

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevHabits.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_[EntityName] : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "[table_name]",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_[table_name]", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_[table_name]_name",
                table: "[table_name]",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_[table_name]_created_at_utc",
                table: "[table_name]",
                column: "created_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "[table_name]");
        }
    }
}
```

### Adding Complex Value Objects

```csharp
/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<int>(
        name: "frequency_period",
        table: "habits",
        type: "integer",
        nullable: false,
        defaultValue: 0);

    migrationBuilder.AddColumn<int>(
        name: "frequency_times",
        table: "habits",
        type: "integer",
        nullable: false,
        defaultValue: 1);

    migrationBuilder.AddColumn<string>(
        name: "target_unit",
        table: "habits",
        type: "character varying(20)",
        maxLength: 20,
        nullable: true);

    migrationBuilder.AddColumn<decimal>(
        name: "target_value",
        table: "habits",
        type: "numeric(10,2)",
        nullable: true);
}
```

### Data Migration Example

```csharp
/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Schema changes first
    migrationBuilder.AddColumn<string>(
        name: "new_column",
        table: "habits",
        type: "character varying(50)",
        maxLength: 50,
        nullable: true);

    // Data migration
    migrationBuilder.Sql(@"
        UPDATE habits 
        SET new_column = 'default_value' 
        WHERE new_column IS NULL;
    ");

    // Make column required after data migration
    migrationBuilder.AlterColumn<string>(
        name: "new_column",
        table: "habits",
        type: "character varying(50)",
        maxLength: 50,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "character varying(50)",
        oldMaxLength: 50,
        oldNullable: true);
}
```

### Many-to-Many Relationship Migration

```csharp
/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "habit_tags",
        columns: table => new
        {
            habit_id = table.Column<string>(type: "character varying(36)", nullable: false),
            tag_id = table.Column<string>(type: "character varying(36)", nullable: false),
            created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("pk_habit_tags", x => new { x.habit_id, x.tag_id });
            table.ForeignKey(
                name: "fk_habit_tags_habits_habit_id",
                column: x => x.habit_id,
                principalTable: "habits",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                name: "fk_habit_tags_tags_tag_id",
                column: x => x.tag_id,
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "ix_habit_tags_tag_id",
        table: "habit_tags",
        column: "tag_id");
}
```

## Performance Optimizations

### Adding Indexes for Common Queries

```csharp
/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Single column indexes
    migrationBuilder.CreateIndex(
        name: "ix_habits_status",
        table: "habits",
        column: "status");

    // Composite indexes for complex queries
    migrationBuilder.CreateIndex(
        name: "ix_habits_user_status_created",
        table: "habits",
        columns: new[] { "user_id", "status", "created_at_utc" });

    // Partial indexes for filtered queries
    migrationBuilder.Sql(@"
        CREATE INDEX ix_habits_active_created 
        ON habits (created_at_utc) 
        WHERE status = 0;
    ");
}
```

## Migration Best Practices

1. **Descriptive Names**: Use clear, descriptive migration names
2. **Small Changes**: Keep migrations focused on single changes
3. **Data Safety**: Always backup before applying migrations
4. **Rollback Plan**: Ensure Down() method can reverse changes
5. **Test Migrations**: Test on development database first
6. **Index Strategy**: Add indexes for performance-critical queries
7. **Data Migrations**: Separate data changes from schema changes
8. **Environment Consideration**: Different strategies for dev/prod
9. **Documentation**: Comment complex migrations
10. **Version Control**: Never modify existing migrations

## DbContext Registration

```csharp
// In Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("DevHabits.Api");
        npgsqlOptions.CommandTimeout(30);
    });
    
    options.UseSnakeCaseNamingConvention();
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```

## Troubleshooting Common Issues

### Migration Conflicts

```bash
# Reset migrations (development only)
dotnet ef database drop --project src/DevHabits.Api
dotnet ef migrations remove --project src/DevHabits.Api
dotnet ef migrations add InitialCreate --project src/DevHabits.Api
dotnet ef database update --project src/DevHabits.Api
```

### Schema Drift

```bash
# Generate script to see differences
dotnet ef migrations script --idempotent --project src/DevHabits.Api
```

### Rollback Strategy

```bash
# Rollback to specific migration
dotnet ef database update [PreviousMigrationName] --project src/DevHabits.Api

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/DevHabits.Api
```
