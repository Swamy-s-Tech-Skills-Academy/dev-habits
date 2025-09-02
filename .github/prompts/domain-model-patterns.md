# Domain Model Patterns

When working with the DevHabits domain model, follow these comprehensive patterns for entities, value objects, and business logic:

## Core Domain Entities

### Habit Entity Template

```csharp
using DevHabits.Api.Entities.ValueObjects;

namespace DevHabits.Api.Entities;

/// <summary>
/// Represents a user's habit with tracking capabilities
/// </summary>
public sealed class Habit
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HabitType Type { get; set; }
    public HabitStatus Status { get; set; }
    
    // Value Objects
    public Frequency Frequency { get; set; } = null!;
    public Target? Target { get; set; }
    public List<Milestone> Milestones { get; set; } = [];
    
    // Navigation Properties
    public List<HabitTag> HabitTags { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
    public List<HabitLog> HabitLogs { get; set; } = [];
    
    // Audit Properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Business Logic Methods
    public bool CanBeCompleted()
    {
        return Status == HabitStatus.Ongoing;
    }

    public bool HasTarget()
    {
        return Type == HabitType.Measurable && Target != null;
    }

    public decimal GetCompletionPercentage()
    {
        if (!HasTarget() || Target!.Value <= 0)
            return 0;

        var totalProgress = HabitLogs
            .Where(log => log.LoggedAtUtc.Date == DateTime.UtcNow.Date)
            .Sum(log => log.Value);

        return Math.Min(100, (totalProgress / Target.Value) * 100);
    }

    public Milestone? GetNextMilestone()
    {
        var currentProgress = GetCurrentProgress();
        return Milestones
            .Where(m => m.Target > currentProgress)
            .OrderBy(m => m.Target)
            .FirstOrDefault();
    }

    public bool IsCompletedToday()
    {
        if (Type == HabitType.Binary)
        {
            return HabitLogs.Any(log => log.LoggedAtUtc.Date == DateTime.UtcNow.Date);
        }

        if (HasTarget())
        {
            return GetCompletionPercentage() >= 100;
        }

        return false;
    }

    private decimal GetCurrentProgress()
    {
        return HabitLogs
            .Where(log => log.LoggedAtUtc.Date == DateTime.UtcNow.Date)
            .Sum(log => log.Value);
    }
}
```

### Tag Entity

```csharp
namespace DevHabits.Api.Entities;

/// <summary>
/// Represents a categorization tag for habits
/// </summary>
public sealed class Tag
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation Properties
    public List<HabitTag> HabitTags { get; set; } = [];
    public List<Habit> Habits { get; set; } = [];
    
    // Audit Properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Business Logic Methods
    public bool IsValidColor()
    {
        var validColors = new[]
        {
            "red", "orange", "yellow", "green", "blue", 
            "indigo", "purple", "pink", "gray", "brown"
        };
        return validColors.Contains(Color.ToLower());
    }

    public int GetHabitCount()
    {
        return HabitTags.Count;
    }
}
```

### Junction Entity (Many-to-Many)

```csharp
namespace DevHabits.Api.Entities;

/// <summary>
/// Junction entity for the many-to-many relationship between Habits and Tags
/// </summary>
public sealed class HabitTag
{
    public string HabitId { get; set; } = string.Empty;
    public string TagId { get; set; } = string.Empty;
    
    // Navigation Properties
    public Habit Habit { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
    
    // Audit Properties
    public DateTime CreatedAtUtc { get; set; }
}
```

### Activity Log Entity

```csharp
namespace DevHabits.Api.Entities;

/// <summary>
/// Represents a logged activity/progress entry for a habit
/// </summary>
public sealed class HabitLog
{
    public string Id { get; set; } = string.Empty;
    public string HabitId { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public DateTime LoggedAtUtc { get; set; }
    
    // Navigation Properties
    public Habit Habit { get; set; } = null!;
    
    // Audit Properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Business Logic Methods
    public bool IsValidForHabitType(HabitType habitType)
    {
        return habitType switch
        {
            HabitType.Binary => Value == 1,
            HabitType.Measurable => Value > 0,
            _ => false
        };
    }

    public bool IsToday()
    {
        return LoggedAtUtc.Date == DateTime.UtcNow.Date;
    }
}
```

## Value Objects

### Frequency Value Object

```csharp
namespace DevHabits.Api.Entities.ValueObjects;

/// <summary>
/// Value object representing habit frequency
/// </summary>
public sealed class Frequency
{
    public FrequencyPeriod Period { get; set; }
    public int Times { get; set; }

    public Frequency() { }

    public Frequency(FrequencyPeriod period, int times)
    {
        Period = period;
        Times = times;
    }

    public bool IsDaily() => Period == FrequencyPeriod.Daily;
    public bool IsWeekly() => Period == FrequencyPeriod.Weekly;
    public bool IsMonthly() => Period == FrequencyPeriod.Monthly;

    public string ToDisplayString()
    {
        return Period switch
        {
            FrequencyPeriod.Daily when Times == 1 => "Daily",
            FrequencyPeriod.Daily => $"{Times} times daily",
            FrequencyPeriod.Weekly when Times == 1 => "Weekly",
            FrequencyPeriod.Weekly => $"{Times} times per week",
            FrequencyPeriod.Monthly when Times == 1 => "Monthly",
            FrequencyPeriod.Monthly => $"{Times} times per month",
            _ => $"{Times} times per {Period.ToString().ToLower()}"
        };
    }

    public int GetTargetPerDay()
    {
        return Period switch
        {
            FrequencyPeriod.Daily => Times,
            FrequencyPeriod.Weekly => Times / 7,
            FrequencyPeriod.Monthly => Times / 30,
            _ => 0
        };
    }

    public bool Equals(Frequency? other)
    {
        if (other is null) return false;
        return Period == other.Period && Times == other.Times;
    }

    public override bool Equals(object? obj) => Equals(obj as Frequency);
    public override int GetHashCode() => HashCode.Combine(Period, Times);
}
```

### Target Value Object

```csharp
namespace DevHabits.Api.Entities.ValueObjects;

/// <summary>
/// Value object representing habit target/goal
/// </summary>
public sealed class Target
{
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;

    public Target() { }

    public Target(decimal value, string unit)
    {
        Value = value;
        Unit = unit;
    }

    public bool IsTimeUnit() => TimeUnits.Contains(Unit.ToLower());
    public bool IsDistanceUnit() => DistanceUnits.Contains(Unit.ToLower());
    public bool IsCountUnit() => CountUnits.Contains(Unit.ToLower());

    public string ToDisplayString()
    {
        return Unit.ToLower() switch
        {
            "minutes" when Value == 1 => "1 minute",
            "minutes" => $"{Value} minutes",
            "hours" when Value == 1 => "1 hour",
            "hours" => $"{Value} hours",
            "steps" => $"{Value:N0} steps",
            "km" => $"{Value} km",
            "miles" => $"{Value} miles",
            "cal" => $"{Value:N0} calories",
            "pages" when Value == 1 => "1 page",
            "pages" => $"{Value} pages",
            "books" when Value == 1 => "1 book",
            "books" => $"{Value} books",
            _ => $"{Value} {Unit}"
        };
    }

    public decimal ConvertToBaseUnit()
    {
        return Unit.ToLower() switch
        {
            "hours" => Value * 60, // Convert to minutes
            "miles" => Value * 1.60934m, // Convert to km
            _ => Value
        };
    }

    public bool IsValidForHabitType(HabitType habitType)
    {
        return habitType switch
        {
            HabitType.Binary => BinaryUnits.Contains(Unit.ToLower()),
            HabitType.Measurable => MeasurableUnits.Contains(Unit.ToLower()),
            _ => false
        };
    }

    private static readonly string[] TimeUnits = ["minutes", "hours"];
    private static readonly string[] DistanceUnits = ["km", "miles"];
    private static readonly string[] CountUnits = ["steps", "cal", "pages", "books"];
    private static readonly string[] BinaryUnits = ["sessions", "tasks"];
    private static readonly string[] MeasurableUnits = ["minutes", "hours", "steps", "km", "miles", "cal", "pages", "books"];

    public bool Equals(Target? other)
    {
        if (other is null) return false;
        return Value == other.Value && Unit == other.Unit;
    }

    public override bool Equals(object? obj) => Equals(obj as Target);
    public override int GetHashCode() => HashCode.Combine(Value, Unit);
}
```

### Milestone Value Object

```csharp
namespace DevHabits.Api.Entities.ValueObjects;

/// <summary>
/// Value object representing a habit milestone
/// </summary>
public sealed class Milestone
{
    public string Name { get; set; } = string.Empty;
    public decimal Target { get; set; }
    public decimal Current { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public Milestone() { }

    public Milestone(string name, decimal target)
    {
        Name = name;
        Target = target;
        Current = 0;
        IsCompleted = false;
    }

    public decimal GetProgressPercentage()
    {
        if (Target <= 0) return 0;
        return Math.Min(100, (Current / Target) * 100);
    }

    public void UpdateProgress(decimal progress)
    {
        Current = Math.Max(0, progress);
        
        if (!IsCompleted && Current >= Target)
        {
            IsCompleted = true;
            CompletedAtUtc = DateTime.UtcNow;
        }
        else if (IsCompleted && Current < Target)
        {
            IsCompleted = false;
            CompletedAtUtc = null;
        }
    }

    public string GetProgressDisplay()
    {
        return $"{Current:N0} / {Target:N0} ({GetProgressPercentage():F1}%)";
    }

    public bool Equals(Milestone? other)
    {
        if (other is null) return false;
        return Name == other.Name && Target == other.Target;
    }

    public override bool Equals(object? obj) => Equals(obj as Milestone);
    public override int GetHashCode() => HashCode.Combine(Name, Target);
}
```

## Enumerations

### Core Enums

```csharp
namespace DevHabits.Api.Entities;

/// <summary>
/// Represents the type of habit tracking
/// </summary>
public enum HabitType
{
    /// <summary>
    /// Simple yes/no completion tracking
    /// </summary>
    Binary = 0,
    
    /// <summary>
    /// Quantifiable progress tracking with specific targets
    /// </summary>
    Measurable = 1
}

/// <summary>
/// Represents the current status of a habit
/// </summary>
public enum HabitStatus
{
    /// <summary>
    /// Habit is actively being tracked
    /// </summary>
    Ongoing = 0,
    
    /// <summary>
    /// Habit has been completed/achieved
    /// </summary>
    Completed = 1,
    
    /// <summary>
    /// Habit is archived/paused
    /// </summary>
    Archived = 2
}

/// <summary>
/// Represents the frequency period for habit repetition
/// </summary>
public enum FrequencyPeriod
{
    /// <summary>
    /// Habit is performed daily
    /// </summary>
    Daily = 0,
    
    /// <summary>
    /// Habit is performed weekly
    /// </summary>
    Weekly = 1,
    
    /// <summary>
    /// Habit is performed monthly
    /// </summary>
    Monthly = 2
}
```

## Domain Services

### Habit Progress Service

```csharp
namespace DevHabits.Api.Services;

/// <summary>
/// Domain service for calculating habit progress and statistics
/// </summary>
public sealed class HabitProgressService
{
    public HabitProgressDto CalculateProgress(Habit habit, List<HabitLog> logs)
    {
        var today = DateTime.UtcNow.Date;
        var todayLogs = logs.Where(l => l.LoggedAtUtc.Date == today).ToList();
        
        return new HabitProgressDto
        {
            HabitId = habit.Id,
            TodayProgress = CalculateTodayProgress(habit, todayLogs),
            WeekProgress = CalculateWeekProgress(habit, logs),
            MonthProgress = CalculateMonthProgress(habit, logs),
            Streak = CalculateStreak(habit, logs),
            CompletionRate = CalculateCompletionRate(habit, logs),
            NextMilestone = habit.GetNextMilestone()?.Name,
            IsCompletedToday = habit.IsCompletedToday()
        };
    }

    private static decimal CalculateTodayProgress(Habit habit, List<HabitLog> todayLogs)
    {
        if (habit.Type == HabitType.Binary)
        {
            return todayLogs.Any() ? 100 : 0;
        }

        if (habit.HasTarget())
        {
            var totalValue = todayLogs.Sum(l => l.Value);
            return Math.Min(100, (totalValue / habit.Target!.Value) * 100);
        }

        return 0;
    }

    private static decimal CalculateWeekProgress(Habit habit, List<HabitLog> logs)
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-((int)DateTime.UtcNow.DayOfWeek));
        var weekLogs = logs.Where(l => l.LoggedAtUtc.Date >= weekStart).ToList();
        
        if (habit.Frequency.IsWeekly())
        {
            var requiredDays = habit.Frequency.Times;
            var completedDays = GetCompletedDaysInPeriod(habit, weekLogs);
            return Math.Min(100, (completedDays / (decimal)requiredDays) * 100);
        }

        return CalculateAverageProgress(habit, weekLogs, 7);
    }

    private static decimal CalculateMonthProgress(Habit habit, List<HabitLog> logs)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthLogs = logs.Where(l => l.LoggedAtUtc.Date >= monthStart).ToList();
        
        if (habit.Frequency.IsMonthly())
        {
            var requiredDays = habit.Frequency.Times;
            var completedDays = GetCompletedDaysInPeriod(habit, monthLogs);
            return Math.Min(100, (completedDays / (decimal)requiredDays) * 100);
        }

        var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        return CalculateAverageProgress(habit, monthLogs, daysInMonth);
    }

    private static int CalculateStreak(Habit habit, List<HabitLog> logs)
    {
        var streak = 0;
        var currentDate = DateTime.UtcNow.Date;
        
        while (true)
        {
            var dayLogs = logs.Where(l => l.LoggedAtUtc.Date == currentDate).ToList();
            
            if (IsCompletedOnDay(habit, dayLogs))
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    private static decimal CalculateCompletionRate(Habit habit, List<HabitLog> logs)
    {
        var last30Days = DateTime.UtcNow.Date.AddDays(-30);
        var recentLogs = logs.Where(l => l.LoggedAtUtc.Date >= last30Days).ToList();
        
        var completedDays = GetCompletedDaysInPeriod(habit, recentLogs);
        return (completedDays / 30m) * 100;
    }

    private static bool IsCompletedOnDay(Habit habit, List<HabitLog> dayLogs)
    {
        if (habit.Type == HabitType.Binary)
        {
            return dayLogs.Any();
        }

        if (habit.HasTarget())
        {
            var totalValue = dayLogs.Sum(l => l.Value);
            return totalValue >= habit.Target!.Value;
        }

        return false;
    }

    private static int GetCompletedDaysInPeriod(Habit habit, List<HabitLog> logs)
    {
        return logs
            .GroupBy(l => l.LoggedAtUtc.Date)
            .Count(group => IsCompletedOnDay(habit, group.ToList()));
    }

    private static decimal CalculateAverageProgress(Habit habit, List<HabitLog> logs, int totalDays)
    {
        var completedDays = GetCompletedDaysInPeriod(habit, logs);
        var expectedCompletions = habit.Frequency.GetTargetPerDay() * totalDays;
        
        if (expectedCompletions <= 0) return 0;
        
        return Math.Min(100, (completedDays / (decimal)expectedCompletions) * 100);
    }
}
```

## Domain Events (Future Enhancement)

### Domain Event Base

```csharp
namespace DevHabits.Api.Domain.Events;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract record DomainEvent
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when a habit is completed for the day
/// </summary>
public sealed record HabitCompletedEvent(
    string HabitId,
    string HabitName,
    DateTime CompletedAtUtc) : DomainEvent;

/// <summary>
/// Event raised when a milestone is achieved
/// </summary>
public sealed record MilestoneAchievedEvent(
    string HabitId,
    string MilestoneName,
    decimal Target,
    DateTime AchievedAtUtc) : DomainEvent;

/// <summary>
/// Event raised when a streak is broken
/// </summary>
public sealed record StreakBrokenEvent(
    string HabitId,
    int PreviousStreak,
    DateTime BrokenAtUtc) : DomainEvent;
```

## Business Rules and Invariants

### Domain Validation Rules

```csharp
namespace DevHabits.Api.Domain.Rules;

/// <summary>
/// Business rules for habit domain
/// </summary>
public static class HabitBusinessRules
{
    public static bool CanLogProgress(Habit habit, decimal value, string? unit)
    {
        // Rule: Can't log progress for archived habits
        if (habit.Status == HabitStatus.Archived)
            return false;

        // Rule: Binary habits can only log value of 1
        if (habit.Type == HabitType.Binary && value != 1)
            return false;

        // Rule: Measurable habits must have positive values
        if (habit.Type == HabitType.Measurable && value <= 0)
            return false;

        // Rule: Unit must match habit's target unit for measurable habits
        if (habit.HasTarget() && !string.IsNullOrEmpty(unit) && 
            !string.Equals(habit.Target!.Unit, unit, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    public static bool CanChangeHabitType(Habit habit, HabitType newType)
    {
        // Rule: Can't change type if habit has logged progress
        if (habit.HabitLogs.Any())
            return false;

        return true;
    }

    public static bool CanDeleteHabit(Habit habit)
    {
        // Rule: Can delete habits with no progress or only draft habits
        return habit.Status != HabitStatus.Completed || !habit.HabitLogs.Any();
    }

    public static bool IsValidFrequency(Frequency frequency)
    {
        // Rule: Frequency times must be between 1 and 10
        if (frequency.Times < 1 || frequency.Times > 10)
            return false;

        // Rule: Daily habits can't exceed 5 times per day
        if (frequency.Period == FrequencyPeriod.Daily && frequency.Times > 5)
            return false;

        return true;
    }

    public static bool IsValidMilestoneProgression(List<Milestone> milestones)
    {
        if (milestones.Count <= 1) return true;

        var sortedMilestones = milestones.OrderBy(m => m.Target).ToList();
        
        for (int i = 1; i < sortedMilestones.Count; i++)
        {
            if (sortedMilestones[i].Target <= sortedMilestones[i - 1].Target)
                return false;
        }

        return true;
    }
}
```

## Best Practices

1. **Encapsulation**: Keep business logic within entities and value objects
2. **Immutability**: Use value objects for immutable concepts like Target and Frequency
3. **Rich Models**: Include behavior in entities, not just data
4. **Domain Services**: Extract complex business logic into domain services
5. **Business Rules**: Centralize business rules in dedicated classes
6. **Validation**: Implement domain validation separate from input validation
7. **Events**: Use domain events for cross-aggregate communication
8. **Invariants**: Maintain entity invariants through constructors and methods
9. **Value Objects**: Use value objects for concepts without identity
10. **Aggregates**: Design aggregates around business transactions
