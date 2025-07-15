# DevHabits API - Data Models Reference

## Overview

This document provides detailed information about all data models used in the DevHabits API.

## Request Models

### CreateHabitDto

Used for creating new habits via `POST /api/habits`.

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `name` | string | Yes | The name of the habit |
| `description` | string | No | A detailed description of the habit (nullable) |
| `type` | HabitType | Yes | The type/category of the habit (enumeration) |
| `frequency` | FrequencyDto | Yes | How often the habit should be performed |
| `target` | TargetDto | Yes | The target metrics for the habit |
| `endDate` | string (date) | No | The date when the habit should end (nullable) |
| `milestone` | MilestoneDto | No | Milestone configuration for progress tracking (nullable) |

**Example:**
```json
{
  "name": "Daily Exercise",
  "description": "Exercise for 30 minutes daily to stay healthy",
  "type": 1,
  "frequency": {
    "type": 1,
    "timesPerPeriod": 1
  },
  "target": {
    "value": 30,
    "unit": "minutes"
  },
  "endDate": "2025-12-31",
  "milestone": {
    "target": 100,
    "current": 0
  }
}
```

## Response Models

### HabitDto

Represents a complete habit with all tracking information.

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `id` | string | Yes | Unique identifier for the habit |
| `name` | string | Yes | The name of the habit |
| `description` | string | Yes | Description of the habit (nullable) |
| `type` | HabitType | Yes | The type/category of the habit |
| `frequency` | FrequencyDto | Yes | How often the habit should be performed |
| `target` | TargetDto | Yes | The target metrics for the habit |
| `status` | HabitStatus | Yes | Current status of the habit |
| `isArchived` | boolean | Yes | Whether the habit is archived |
| `endDate` | string (date) | Yes | The date when the habit should end (nullable) |
| `milestone` | MilestoneDto | Yes | Milestone progress (nullable) |
| `createdAtUtc` | string (date-time) | Yes | When the habit was created (UTC) |
| `updatedAtUtc` | string (date-time) | Yes | When the habit was last updated (UTC, nullable) |
| `lastCompletedAtUtc` | string (date-time) | Yes | When the habit was last completed (UTC, nullable) |

**Example:**
```json
{
  "id": "habit-123",
  "name": "Daily Exercise",
  "description": "Exercise for 30 minutes daily to stay healthy",
  "type": 1,
  "frequency": {
    "type": 1,
    "timesPerPeriod": 1
  },
  "target": {
    "value": 30,
    "unit": "minutes"
  },
  "status": 1,
  "isArchived": false,
  "endDate": "2025-12-31",
  "milestone": {
    "target": 100,
    "current": 25
  },
  "createdAtUtc": "2025-07-15T10:00:00Z",
  "updatedAtUtc": "2025-07-15T14:00:00Z",
  "lastCompletedAtUtc": "2025-07-15T08:00:00Z"
}
```

### HabitsCollectionDto

Container for multiple habits returned by `GET /api/habits`.

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `data` | array of HabitDto | Yes | Array containing habit objects |

**Example:**
```json
{
  "data": [
    {
      "id": "habit-123",
      "name": "Daily Exercise",
      // ... other HabitDto properties
    },
    {
      "id": "habit-456",
      "name": "Read Books",
      // ... other HabitDto properties
    }
  ]
}
```

## Nested Models

### FrequencyDto

Defines how often a habit should be performed.

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | FrequencyType | Yes | The type of frequency (enumeration) |
| `timesPerPeriod` | integer | Yes | Number of times per period |

**Example:**
```json
{
  "type": 1,
  "timesPerPeriod": 3
}
```

### TargetDto

Defines the target metrics for a habit.

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `value` | integer | Yes | The target value |
| `unit` | string | Yes | The unit of measurement |

**Example:**
```json
{
  "value": 30,
  "unit": "minutes"
}
```

### MilestoneDto

Tracks progress towards milestones (nullable).

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `target` | integer | Yes | The target milestone value |
| `current` | integer | Yes | The current progress value |

**Example:**
```json
{
  "target": 100,
  "current": 25
}
```

## Enumerations

### HabitType

Integer enumeration representing different types of habits.

**Possible Values:**
- `0`: Unknown/Default
- `1`: Health & Fitness
- `2`: Learning & Education
- `3`: Productivity
- `4`: Personal Development
- `5`: Social & Relationships

*Note: Actual enum values may vary based on implementation.*

### HabitStatus

Integer enumeration representing the current status of a habit.

**Possible Values:**
- `0`: Inactive
- `1`: Active
- `2`: Completed
- `3`: Paused
- `4`: Failed

*Note: Actual enum values may vary based on implementation.*

### FrequencyType

Integer enumeration representing different frequency types.

**Possible Values:**
- `0`: Unknown/Default
- `1`: Daily
- `2`: Weekly
- `3`: Monthly
- `4`: Custom

*Note: Actual enum values may vary based on implementation.*

## Demo Models

### WeatherForecast

Demo model for weather data (used in `/WeatherForecast` endpoint).

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `date` | string (date) | No | Date of the forecast |
| `temperatureC` | integer | No | Temperature in Celsius |
| `temperatureF` | integer | No | Temperature in Fahrenheit |
| `summary` | string | No | Weather summary (nullable) |

**Example:**
```json
{
  "date": "2025-07-15",
  "temperatureC": 25,
  "temperatureF": 77,
  "summary": "Partly cloudy"
}
```

## Data Validation

### Required Fields

All fields marked as "Required: Yes" must be provided in requests and will always be present in responses.

### Nullable Fields

Fields marked as "nullable" can contain `null` values or be omitted from requests.

### Data Formats

- **Date**: ISO 8601 date format (YYYY-MM-DD)
- **DateTime**: ISO 8601 date-time format (YYYY-MM-DDTHH:MM:SSZ)
- **Integer**: 32-bit signed integer
- **String**: UTF-8 encoded text

### Length Constraints

*Note: Specific length constraints are not defined in the OpenAPI specification. Check with the API implementation for actual limits.*

## Error Responses

When validation fails or errors occur, the API will return appropriate HTTP status codes:

- **400 Bad Request**: Invalid request data or validation errors
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server-side errors

Error response format may vary based on the specific error type and API implementation.
