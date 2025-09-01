# Copilot Instructions for DevHabits

## Project Overview

DevHabits is a comprehensive .NET 10.0 API for tracking and managing daily habits with advanced features including:

- **Habit Management**: Create, update, track habits with flexible frequency configurations (daily, weekly, monthly)
- **Progress Monitoring**: Real-time progress tracking with milestone system
- **Tagging System**: Categorize habits with custom tags and many-to-many relationships
- **Target Metrics**: Flexible target system supporting various units (time, distance, quantity)
- **Binary & Measurable Habits**: Support for both yes/no habits and quantifiable activities
- **Status Tracking**: Ongoing, completed, and archived habit states

## Domain Model

### Core Entities

- **Habit**: Main entity with name, description, type (Binary/Measurable), frequency, target, status, milestones
- **Tag**: Categorization labels for habits
- **HabitTag**: Many-to-many relationship between habits and tags
- **Frequency**: Value object defining repetition pattern (Daily, Weekly, Monthly) with TimesPerPeriod
- **Target**: Value object for goal definition with value and unit (minutes, steps, pages, etc.)
- **Milestone**: Value object for progress tracking with target and current values

### Business Rules

- Habit names: 3-100 characters required
- Binary habits: Use sessions/tasks units only
- Measurable habits: Support various units (minutes, hours, steps, km, cal, pages, books)
- Frequency: TimesPerPeriod must be greater than 0
- All entities have audit fields (CreatedAtUtc, UpdatedAtUtc)
- Entity IDs use Version 7 GUIDs with prefixes (h* for habits, t* for tags)
- Value objects (Frequency, Target, Milestone) are owned entities

## Architecture & Technology Stack

### Framework & Runtime

- **.NET 10.0** with C# 13 features
- **ASP.NET Core** Web API with minimal hosting model
- **Primary constructors** and **record types** for DTOs
- **Nullable reference types** enabled globally

### Data Layer

- **PostgreSQL** as primary database
- **Entity Framework Core 9.0** with migrations
- **Snake_case naming convention** via EFCore.NamingConventions
- **Configuration-based** entity setup (no attributes on entities)
- **String-based GUIDs** for primary keys

### API Design

- **RESTful endpoints** with proper HTTP status codes
- **Content negotiation** supporting JSON, XML, and plain text
- **FluentValidation** for comprehensive input validation
- **OpenAPI 3.0** with Scalar UI for interactive documentation
- **Versioning ready** architecture

### Observability & Monitoring

- **OpenTelemetry** for distributed tracing and metrics
- **Structured logging** with correlation IDs
- **Runtime, HTTP, and database instrumentation**
- **Azure Monitor** integration ready

### Quality & Analysis

- **SonarAnalyzer.CSharp** for code quality
- **Warnings as errors** enforcement
- **Code style analysis** in build
- **EditorConfig** for consistent formatting

## Coding Standards

### C# Conventions

- **PascalCase**: Public members, classes, methods, properties
- **camelCase**: Private fields, local variables, parameters
- **UPPER_CASE**: Constants and static readonly fields
- **Async suffix**: For async methods (CreateHabitAsync)
- **Primary constructors** for simple dependency injection
- **Record types** for immutable DTOs with `required` properties

### Documentation Standards

```csharp
/// <summary>
/// Creates a new habit with the provided details
/// </summary>
/// <param name="createHabitDto">The habit data to create</param>
/// <returns>The created habit with generated ID</returns>
/// <response code="201">Returns the newly created habit</response>
/// <response code="400">If the habit data is invalid</response>
[HttpPost]
[ProducesResponseType<HabitDto>(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<HabitDto>> CreateHabitAsync(CreateHabitDto createHabitDto)
```

### Entity Design Patterns

```csharp
// Entities: Simple POCOs with navigation properties
public sealed class Habit
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    // ... other properties
    public List<HabitTag> HabitTags { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
}

// DTOs: Immutable records with required properties
public sealed record CreateHabitDto
{
    public required string Name { get; init; }
    public required HabitType Type { get; init; }
    // ... other properties
}
```

### Validation Patterns

```csharp
public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits = ["minutes", "hours", "steps"];

    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        // Custom business rule validation
        RuleFor(x => x)
            .Must(BeValidUnitForHabitType)
            .WithMessage("Invalid unit for habit type");
    }
}
```

## Database Guidelines

### Migration Practices

- **Descriptive names**: `Add_Habits`, `Update_HabitFrequency`
- **Data migrations**: Separate from schema changes when needed
- **Rollback safety**: Ensure migrations can be reverted
- **Index strategy**: Add indexes for frequently queried columns

### Entity Configuration

```csharp
public sealed class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasMaxLength(36);
        builder.Property(h => h.Name).HasMaxLength(100).IsRequired();

        // Complex type mapping
        builder.OwnsOne(h => h.Frequency);
        builder.OwnsOne(h => h.Target);
    }
}
```

## API Development Guidelines

### Controller Patterns

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    // Use async/await for all I/O operations
    // Include proper response type attributes
    // Add XML documentation for OpenAPI
}
```

### Error Handling

- **ProblemDetails** for consistent error responses
- **Custom exceptions** for business rule violations
- **Global exception handling** middleware
- **Correlation IDs** for request tracing

## Testing Strategy (To Be Implemented)

### Unit Tests Structure

```
tests/
├── DevHabits.Api.UnitTests/
│   ├── Controllers/
│   ├── DTOs/Validators/
│   ├── Entities/
│   └── Services/
├── DevHabits.Api.IntegrationTests/
│   ├── Controllers/
│   ├── Database/
│   └── TestFixtures/
└── DevHabits.Api.PerformanceTests/
```

### Testing Frameworks

- **xUnit** for unit and integration tests
- **NSubstitute** for mocking
- **Microsoft.AspNetCore.Mvc.Testing** for integration tests
- **Microsoft.EntityFrameworkCore.InMemory** for database testing

## Docker & Deployment

### Container Strategy

- **Multi-stage Dockerfile** for optimized images
- **Development**: docker-compose with hot reload
- **Production**: Health checks and proper logging
- **PostgreSQL**: Containerized for local development

### Environment Configuration

- **User Secrets** for local development
- **Environment variables** for containerized deployments
- **Connection string management** for different environments

## Git & CI/CD

### Commit Conventions

```
feat: add habit milestone tracking
fix: resolve validation issue in CreateHabitDto
docs: update API documentation for tags endpoint
refactor: improve habit query performance
test: add unit tests for habit validation
```

### Branch Strategy

- **main**: Production-ready code
- **feature/\***: New features and enhancements
- **hotfix/\***: Critical bug fixes
- **docs/\***: Documentation updates

### GitHub Actions Pipeline

- **.NET 10.0** setup and restoration
- **Build** with Release configuration
- **Test** execution with coverage
- **Code analysis** with SonarAnalyzer
- **Docker** image building and publishing

## Code Generation Guidelines

When generating code, always:

1. **Follow the established patterns** in the codebase
2. **Include proper validation** using FluentValidation
3. **Add comprehensive XML documentation**
4. **Use primary constructors** for dependency injection
5. **Implement proper error handling**
6. **Add appropriate OpenAPI attributes**
7. **Follow the naming conventions**
8. **Include audit fields** where appropriate
9. **Use nullable reference types** correctly
10. **Write corresponding unit tests**

---

_These instructions ensure GitHub Copilot generates code that aligns with the DevHabits project's architecture, patterns, and quality standards._

- Support both development and production environments.
- Include proper health checks in containers.
- Use docker-compose for local development.

## Observability

- Use OpenTelemetry for tracing and metrics.
- Include structured logging with ILogger.
- Add appropriate log levels (Information, Warning, Error).
- Include correlation IDs for request tracing.

## Security

- Validate all input data.
- Use HTTPS in production.
- Implement proper error handling without exposing sensitive information.
- Follow OWASP security guidelines.

## Git & Development Workflow

- Use conventional commit messages.
- Create feature branches for new development.
- Write clear PR descriptions with context.
- Ensure all tests pass before merging.
- Update documentation with code changes.

## Documentation

- Keep API documentation up to date in the `docs/` folder.
- Update OpenAPI specifications when endpoints change.
- Include code examples in documentation.
- Maintain README files for setup instructions.

---

_These instructions help GitHub Copilot understand the DevHabits project context and coding standards._
