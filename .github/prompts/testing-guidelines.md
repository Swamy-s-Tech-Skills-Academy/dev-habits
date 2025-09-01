# Testing Prompt

When creating tests for the DevHabits project, follow these guidelines:

## Test Project Structure

```
tests/
├── DevHabits.Api.UnitTests/
│   ├── Controllers/
│   │   ├── HabitsControllerTests.cs
│   │   └── TagsControllerTests.cs
│   ├── DTOs/
│   │   ├── Validators/
│   │   │   ├── CreateHabitDtoValidatorTests.cs
│   │   │   └── UpdateHabitDtoValidatorTests.cs
│   │   └── Mappings/
│   │       ├── HabitMappingsTests.cs
│   │       └── TagMappingsTests.cs
│   ├── Entities/
│   │   ├── HabitTests.cs
│   │   └── TagTests.cs
│   └── Services/
├── DevHabits.Api.IntegrationTests/
│   ├── Controllers/
│   │   ├── HabitsControllerIntegrationTests.cs
│   │   └── TagsControllerIntegrationTests.cs
│   ├── Database/
│   │   └── ApplicationDbContextTests.cs
│   └── TestFixtures/
│       ├── WebApplicationFactory.cs
│       └── DatabaseFixture.cs
└── DevHabits.Api.PerformanceTests/
    ├── LoadTests.cs
    └── StressTests.cs
```

## Unit Test Template

```csharp
using DevHabits.Api.Controllers;
using DevHabits.Api.Database;
using DevHabits.Api.DTOs.[EntityName]s;
using DevHabits.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace DevHabits.Api.UnitTests.Controllers;

/// <summary>
/// Unit tests for [EntityName]sController
/// </summary>
public sealed class [EntityName]sControllerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly [EntityName]sController _controller;

    public [EntityName]sControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _controller = new [EntityName]sController(_dbContext);
    }

    [Fact]
    public async Task Get[EntityName]sAsync_ReturnsOkResult_WithCollectionOf[EntityName]s()
    {
        // Arrange
        var entities = new List<[EntityName]>
        {
            CreateTestEntity("Test 1"),
            CreateTestEntity("Test 2")
        };

        _dbContext.[EntityName]s.AddRange(entities);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.Get[EntityName]sAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var collection = Assert.IsType<[EntityName]sCollectionDto>(okResult.Value);
        Assert.Equal(2, collection.Data.Count);
    }

    [Fact]
    public async Task Get[EntityName]ByIdAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var entity = CreateTestEntity("Test Entity");
        _dbContext.[EntityName]s.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.Get[EntityName]ByIdAsync(entity.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<[EntityName]Dto>(okResult.Value);
        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Name, dto.Name);
    }

    [Fact]
    public async Task Get[EntityName]ByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Get[EntityName]ByIdAsync("nonexistent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create[EntityName]Async_WithValidDto_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new Create[EntityName]Dto
        {
            Name = "Test Entity",
            Description = "Test Description"
            // Add required properties
        };

        // Act
        var result = await _controller.Create[EntityName]Async(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<[EntityName]Dto>(createdResult.Value);
        Assert.Equal(createDto.Name, dto.Name);
        Assert.Equal(createDto.Description, dto.Description);

        // Verify entity was saved to database
        var savedEntity = await _dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == dto.Id);
        Assert.NotNull(savedEntity);
    }

    [Fact]
    public async Task Update[EntityName]Async_WithValidDto_ReturnsNoContent()
    {
        // Arrange
        var entity = CreateTestEntity("Original Name");
        _dbContext.[EntityName]s.Add(entity);
        await _dbContext.SaveChangesAsync();

        var updateDto = new Update[EntityName]Dto
        {
            Name = "Updated Name",
            Description = "Updated Description"
            // Add required properties
        };

        // Act
        var result = await _controller.Update[EntityName]Async(entity.Id, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify entity was updated
        var updatedEntity = await _dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.NotNull(updatedEntity);
        Assert.Equal(updateDto.Name, updatedEntity.Name);
        Assert.NotNull(updatedEntity.UpdatedAtUtc);
    }

    [Fact]
    public async Task Delete[EntityName]Async_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var entity = CreateTestEntity("Test Entity");
        _dbContext.[EntityName]s.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.Delete[EntityName]Async(entity.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify entity was deleted
        var deletedEntity = await _dbContext.[EntityName]s.FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.Null(deletedEntity);
    }

    private static [EntityName] CreateTestEntity(string name)
    {
        return new [EntityName]
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = "Test Description",
            CreatedAtUtc = DateTime.UtcNow
            // Add required properties
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
```

## Validator Test Template

```csharp
using DevHabits.Api.DTOs.[EntityName]s;
using FluentValidation.TestHelper;

namespace DevHabits.Api.UnitTests.DTOs.Validators;

/// <summary>
/// Unit tests for Create[EntityName]DtoValidator
/// </summary>
public sealed class Create[EntityName]DtoValidatorTests
{
    private readonly Create[EntityName]DtoValidator _validator;

    public Create[EntityName]DtoValidatorTests()
    {
        _validator = new Create[EntityName]DtoValidator();
    }

    [Fact]
    public void Validate_ValidDto_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var dto = new Create[EntityName]Dto
        {
            Name = "Valid Name",
            Description = "Valid Description"
            // Add required properties
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_EmptyName_ShouldHaveValidationError(string name)
    {
        // Arrange
        var dto = new Create[EntityName]Dto
        {
            Name = name,
            Description = "Valid Description"
            // Add required properties
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("ab")] // Too short
    [InlineData("a")] // Too short
    public void Validate_NameTooShort_ShouldHaveValidationError(string name)
    {
        // Arrange
        var dto = new Create[EntityName]Dto
        {
            Name = name,
            Description = "Valid Description"
            // Add required properties
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must be between 3 and 100 characters");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new Create[EntityName]Dto
        {
            Name = new string('a', 101), // Too long
            Description = "Valid Description"
            // Add required properties
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new Create[EntityName]Dto
        {
            Name = "Valid Name",
            Description = new string('a', 501) // Too long
            // Add required properties
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
```

## Integration Test Template

```csharp
using DevHabits.Api.DTOs.[EntityName]s;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DevHabits.Api.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for [EntityName]sController
/// </summary>
public sealed class [EntityName]sControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public [EntityName]sControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get[EntityName]s_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/[entityname]s");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Create[EntityName]_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new Create[EntityName]Dto
        {
            Name = "Integration Test Entity",
            Description = "Created by integration test"
            // Add required properties
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/[entityname]s", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdEntity = JsonSerializer.Deserialize<[EntityName]Dto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(createdEntity);
        Assert.Equal(createDto.Name, createdEntity.Name);
        Assert.Equal(createDto.Description, createdEntity.Description);
    }

    [Fact]
    public async Task Create[EntityName]_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidDto = new Create[EntityName]Dto
        {
            Name = "", // Invalid: empty name
            Description = "Test Description"
            // Add required properties with invalid values
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/[entityname]s", invalidDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get[EntityName]ById_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/[entityname]s/nonexistent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

## Test Fixtures

### Web Application Factory

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DevHabits.Api.Database;

namespace DevHabits.Api.IntegrationTests.TestFixtures;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a database context using an in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
}
```

## Best Practices

1. **Test Naming**: Use descriptive names that indicate what is being tested
2. **Arrange-Act-Assert**: Follow the AAA pattern
3. **One Assertion Per Test**: Focus on testing one thing at a time
4. **Test Data Builders**: Use factory methods for creating test data
5. **Async Tests**: Use async/await for async methods
6. **Dispose Resources**: Implement IDisposable for cleanup
7. **Theory Tests**: Use `[Theory]` and `[InlineData]` for parameterized tests
8. **Integration Tests**: Test the full request/response cycle
9. **Mocking**: Use NSubstitute for mocking dependencies
10. **Code Coverage**: Aim for high coverage on business logic

## xUnit Project Template

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" />
    <PackageReference Include="FluentValidation.TestHelper" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\DevHabits.Api\DevHabits.Api.csproj" />
  </ItemGroup>

</Project>
```
