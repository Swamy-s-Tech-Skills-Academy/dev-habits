# Validation Patterns

When implementing validation for the DevHabits project, follow these comprehensive patterns using FluentValidation:

## Basic Validator Template

```csharp
using DevHabits.Api.DTOs.[EntityName]s;
using FluentValidation;

namespace DevHabits.Api.DTOs.[EntityName]s;

/// <summary>
/// Validator for Create[EntityName]Dto
/// </summary>
public sealed class Create[EntityName]DtoValidator : AbstractValidator<Create[EntityName]Dto>
{
    public Create[EntityName]DtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(3)
            .WithMessage("Name must be at least 3 characters long")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        // Add custom validation rules
        RuleFor(x => x)
            .Must(BeValidBusinessRule)
            .WithMessage("Custom business rule validation failed");
    }

    private static bool BeValidBusinessRule(Create[EntityName]Dto dto)
    {
        // Implement custom business logic
        return true;
    }
}
```

## Common Validation Patterns

### String Validation

```csharp
public sealed class StringValidationExamples : AbstractValidator<ExampleDto>
{
    public StringValidationExamples()
    {
        // Required string
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        // String length constraints
        RuleFor(x => x.Name)
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Name must be between 3 and 100 characters");

        // String format validation
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email format");

        // Custom regex pattern
        RuleFor(x => x.Code)
            .Matches(@"^[A-Z]{2}-\d{4}$")
            .WithMessage("Code must be in format XX-9999");

        // Whitespace handling
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name cannot be empty or whitespace");

        // Allowed values
        RuleFor(x => x.Category)
            .Must(category => AllowedCategories.Contains(category))
            .WithMessage($"Category must be one of: {string.Join(", ", AllowedCategories)}");
    }

    private static readonly string[] AllowedCategories = ["Health", "Productivity", "Learning"];
}
```

### Numeric Validation

```csharp
public sealed class NumericValidationExamples : AbstractValidator<ExampleDto>
{
    public NumericValidationExamples()
    {
        // Range validation
        RuleFor(x => x.Age)
            .InclusiveBetween(1, 120)
            .WithMessage("Age must be between 1 and 120");

        // Greater than validation
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        // Less than or equal validation
        RuleFor(x => x.Percentage)
            .LessThanOrEqualTo(100)
            .WithMessage("Percentage cannot exceed 100");

        // Decimal precision
        RuleFor(x => x.Price)
            .ScalePrecision(2, 10)
            .WithMessage("Price must have at most 2 decimal places");

        // Conditional numeric validation
        RuleFor(x => x.Discount)
            .GreaterThan(0)
            .When(x => x.HasDiscount)
            .WithMessage("Discount must be greater than 0 when HasDiscount is true");
    }
}
```

### Enum Validation

```csharp
public sealed class EnumValidationExamples : AbstractValidator<ExampleDto>
{
    public EnumValidationExamples()
    {
        // Enum value validation
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid status value");

        // Specific enum values
        RuleFor(x => x.Priority)
            .Must(priority => priority == Priority.High || priority == Priority.Medium)
            .WithMessage("Priority must be High or Medium");

        // Enum with custom validation
        RuleFor(x => x.HabitType)
            .Must(BeValidHabitType)
            .WithMessage("Invalid habit type for the current context");
    }

    private static bool BeValidHabitType(HabitType habitType)
    {
        return Enum.IsDefined(typeof(HabitType), habitType);
    }
}
```

### DateTime Validation

```csharp
public sealed class DateTimeValidationExamples : AbstractValidator<ExampleDto>
{
    public DateTimeValidationExamples()
    {
        // Date range validation
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        // Date format validation (for strings)
        RuleFor(x => x.DateString)
            .Must(BeValidDateFormat)
            .WithMessage("Date must be in YYYY-MM-DD format");

        // Age validation
        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.UtcNow.AddYears(-18))
            .WithMessage("Must be at least 18 years old");
    }

    private static bool BeValidDateFormat(string dateString)
    {
        return DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, DateTimeStyles.None, out _);
    }
}
```

### Collection Validation

```csharp
public sealed class CollectionValidationExamples : AbstractValidator<ExampleDto>
{
    public CollectionValidationExamples()
    {
        // Collection not empty
        RuleFor(x => x.Tags)
            .NotEmpty()
            .WithMessage("At least one tag is required");

        // Collection size limits
        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 10)
            .WithMessage("Cannot have more than 10 tags");

        // Validate each item in collection
        RuleForEach(x => x.Tags)
            .SetValidator(new TagValidator());

        // Collection uniqueness
        RuleFor(x => x.Categories)
            .Must(HaveUniqueValues)
            .WithMessage("Categories must be unique");

        // Complex collection validation
        RuleFor(x => x.Milestones)
            .Must(BeValidMilestoneProgression)
            .WithMessage("Milestones must be in ascending order");
    }

    private static bool HaveUniqueValues(IEnumerable<string> categories)
    {
        return categories.Distinct().Count() == categories.Count();
    }

    private static bool BeValidMilestoneProgression(List<MilestoneDto> milestones)
    {
        if (milestones.Count <= 1) return true;
        
        for (int i = 1; i < milestones.Count; i++)
        {
            if (milestones[i].Target <= milestones[i - 1].Target)
                return false;
        }
        return true;
    }
}
```

## DevHabits Domain-Specific Validation

### Habit Validation

```csharp
public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] BinaryHabitUnits = ["sessions", "tasks"];
    private static readonly string[] MeasurableHabitUnits = 
        ["minutes", "hours", "steps", "km", "miles", "cal", "pages", "books"];

    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Habit name is required")
            .MinimumLength(3)
            .WithMessage("Habit name must be at least 3 characters")
            .MaximumLength(100)
            .WithMessage("Habit name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid habit type");

        // Frequency validation
        RuleFor(x => x.Frequency)
            .NotNull()
            .WithMessage("Frequency is required")
            .DependentRules(() =>
            {
                RuleFor(x => x.Frequency.Period)
                    .IsInEnum()
                    .WithMessage("Invalid frequency period");

                RuleFor(x => x.Frequency.Times)
                    .InclusiveBetween(1, 10)
                    .WithMessage("Frequency times must be between 1 and 10");
            });

        // Target validation for measurable habits
        When(x => x.Type == HabitType.Measurable, () =>
        {
            RuleFor(x => x.Target)
                .NotNull()
                .WithMessage("Target is required for measurable habits")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Target!.Value)
                        .GreaterThan(0)
                        .WithMessage("Target value must be greater than 0");

                    RuleFor(x => x.Target!.Unit)
                        .NotEmpty()
                        .WithMessage("Target unit is required")
                        .Must(unit => MeasurableHabitUnits.Contains(unit))
                        .WithMessage($"Target unit must be one of: {string.Join(", ", MeasurableHabitUnits)}");
                });
        });

        // Target validation for binary habits
        When(x => x.Type == HabitType.Binary, () =>
        {
            RuleFor(x => x.Target)
                .Must(target => target == null || BinaryHabitUnits.Contains(target.Unit))
                .WithMessage($"Binary habits can only use units: {string.Join(", ", BinaryHabitUnits)}");
        });

        // Cross-field validation
        RuleFor(x => x)
            .Must(BeValidHabitConfiguration)
            .WithMessage("Invalid habit configuration");

        // Tag validation
        RuleFor(x => x.TagIds)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("Cannot assign more than 10 tags to a habit");

        When(x => x.TagIds != null && x.TagIds.Any(), () =>
        {
            RuleFor(x => x.TagIds!)
                .Must(HaveUniqueTagIds)
                .WithMessage("Tag IDs must be unique");

            RuleForEach(x => x.TagIds!)
                .NotEmpty()
                .WithMessage("Tag ID cannot be empty")
                .Must(BeValidGuid)
                .WithMessage("Tag ID must be a valid GUID");
        });
    }

    private static bool BeValidHabitConfiguration(CreateHabitDto dto)
    {
        // Complex business rule: Binary habits with weekly frequency can't have daily targets
        if (dto.Type == HabitType.Binary && 
            dto.Frequency?.Period == FrequencyPeriod.Weekly &&
            dto.Target?.Unit == "sessions" &&
            dto.Target?.Value > dto.Frequency?.Times)
        {
            return false;
        }

        return true;
    }

    private static bool HaveUniqueTagIds(List<string> tagIds)
    {
        return tagIds.Distinct().Count() == tagIds.Count;
    }

    private static bool BeValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
```

### Tag Validation

```csharp
public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    private static readonly string[] AllowedColors = 
    [
        "red", "orange", "yellow", "green", "blue", "indigo", "purple", "pink",
        "gray", "brown", "cyan", "lime", "amber", "teal", "slate"
    ];

    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tag name is required")
            .MinimumLength(2)
            .WithMessage("Tag name must be at least 2 characters")
            .MaximumLength(50)
            .WithMessage("Tag name cannot exceed 50 characters")
            .Must(BeValidTagName)
            .WithMessage("Tag name can only contain letters, numbers, spaces, and hyphens");

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage("Tag color is required")
            .Must(color => AllowedColors.Contains(color.ToLower()))
            .WithMessage($"Color must be one of: {string.Join(", ", AllowedColors)}");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");
    }

    private static bool BeValidTagName(string name)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-]+$");
    }
}
```

### Update Validation

```csharp
public sealed class UpdateHabitDtoValidator : AbstractValidator<UpdateHabitDto>
{
    public UpdateHabitDtoValidator()
    {
        // Reuse rules from create validator
        Include(new CreateHabitDtoValidator());

        // Additional update-specific rules
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid habit status");

        // Prevent certain changes based on current state
        RuleFor(x => x.Type)
            .Must((dto, type) => CanChangeHabitType(dto, type))
            .WithMessage("Cannot change habit type when habit has progress data");
    }

    private static bool CanChangeHabitType(UpdateHabitDto dto, HabitType newType)
    {
        // Business rule: Can't change type if habit has logged progress
        // This would need to be validated against the database
        return true; // Simplified for example
    }
}
```

## Advanced Validation Patterns

### Conditional Validation

```csharp
public sealed class ConditionalValidationExample : AbstractValidator<ExampleDto>
{
    public ConditionalValidationExample()
    {
        // When condition
        When(x => x.IsEnabled, () =>
        {
            RuleFor(x => x.Configuration)
                .NotNull()
                .WithMessage("Configuration is required when enabled");
        });

        // Unless condition
        Unless(x => x.IsOptional, () =>
        {
            RuleFor(x => x.RequiredField)
                .NotEmpty()
                .WithMessage("Field is required unless marked as optional");
        });

        // Complex conditional logic
        RuleFor(x => x.Value)
            .GreaterThan(0)
            .When(x => x.Type == ValueType.Positive)
            .LessThan(0)
            .When(x => x.Type == ValueType.Negative);
    }
}
```

### Custom Validators

```csharp
public sealed class CustomValidatorExample : AbstractValidator<ExampleDto>
{
    public CustomValidatorExample()
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());

        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator())
            .When(x => x.RequiresAddress);
    }
}

public sealed class EmailValidator : AbstractValidator<string>
{
    public EmailValidator()
    {
        RuleFor(email => email)
            .NotEmpty()
            .EmailAddress()
            .Must(BeValidDomain)
            .WithMessage("Email must be from an allowed domain");
    }

    private static bool BeValidDomain(string email)
    {
        var allowedDomains = new[] { "company.com", "gmail.com", "outlook.com" };
        var domain = email.Split('@').LastOrDefault();
        return allowedDomains.Contains(domain);
    }
}
```

### Async Validation

```csharp
public sealed class AsyncValidationExample : AbstractValidator<ExampleDto>
{
    private readonly IRepository _repository;

    public AsyncValidationExample(IRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Email)
            .MustAsync(BeUniqueEmail)
            .WithMessage("Email address is already in use");

        RuleFor(x => x.Name)
            .MustAsync(BeValidName)
            .WithMessage("Name contains prohibited content");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _repository.EmailExistsAsync(email, cancellationToken);
    }

    private async Task<bool> BeValidName(string name, CancellationToken cancellationToken)
    {
        // Call external service to validate name
        return await ExternalValidationService.ValidateNameAsync(name);
    }
}
```

## Error Message Customization

### Custom Error Messages

```csharp
public sealed class CustomErrorMessagesExample : AbstractValidator<ExampleDto>
{
    public CustomErrorMessagesExample()
    {
        // Simple custom message
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Please provide a name");

        // Message with property name
        RuleFor(x => x.Age)
            .GreaterThan(0)
            .WithMessage("{PropertyName} must be a positive number");

        // Message with property value
        RuleFor(x => x.Code)
            .MinimumLength(3)
            .WithMessage("The value '{PropertyValue}' is too short");

        // Complex custom message
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage(x => $"'{x.Email}' is not a valid email address");

        // Localized messages
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name_Required"); // Key for localization
    }
}
```

### Error Code Assignment

```csharp
public sealed class ErrorCodeExample : AbstractValidator<ExampleDto>
{
    public ErrorCodeExample()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("HABIT_NAME_REQUIRED")
            .WithMessage("Habit name is required");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithErrorCode("INVALID_EMAIL_FORMAT")
            .WithMessage("Invalid email format");
    }
}
```

## Validation Testing

### Validator Unit Tests

```csharp
public sealed class CreateHabitDtoValidatorTests
{
    private readonly CreateHabitDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_ShouldNotHaveErrors()
    {
        var dto = new CreateHabitDto
        {
            Name = "Valid Habit",
            Type = HabitType.Binary,
            Frequency = new FrequencyDto { Period = FrequencyPeriod.Daily, Times = 1 }
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData(null)]
    public void Validate_InvalidName_ShouldHaveError(string name)
    {
        var dto = new CreateHabitDto { Name = name };

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
```

## Best Practices

1. **Specific Messages**: Provide clear, actionable error messages
2. **Business Rules**: Implement domain-specific validation logic
3. **Performance**: Use async validation only when necessary
4. **Consistency**: Use consistent validation patterns across DTOs
5. **Testing**: Write comprehensive validator tests
6. **Localization**: Support multiple languages for error messages
7. **Error Codes**: Use error codes for programmatic error handling
8. **Conditional Logic**: Use When/Unless for complex conditions
9. **Reusability**: Create reusable custom validators
10. **Documentation**: Document complex validation rules
