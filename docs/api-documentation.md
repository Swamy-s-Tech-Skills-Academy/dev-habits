# DevHabits API Documentation

## Overview

The DevHabits API is a RESTful web service that helps users track and manage their daily habits. This API provides endpoints for creating, reading, updating, and managing habits with various tracking metrics.

**Base URL**: `https://localhost:5002/`  
**API Version**: v1  
**OpenAPI Specification**: 3.0.1

## Authentication

Currently, the API does not require authentication (this may change in future versions).

## Content Types

The API supports multiple content types for both requests and responses:

### Supported Request Content Types:

- `application/json`
- `application/json-patch+json`
- `text/json`
- `application/*+json`
- `application/xml`
- `text/xml`
- `application/*+xml`

### Supported Response Content Types:

- `application/json`
- `text/plain`
- `text/json`
- `application/xml`
- `text/xml`

## API Endpoints

### Habits Management

#### Get All Habits

- **Endpoint**: `GET /api/habits`
- **Description**: Retrieves a collection of all habits
- **Response**: `200 OK` with `HabitsCollectionDto`

#### Create New Habit

- **Endpoint**: `POST /api/habits`
- **Description**: Creates a new habit
- **Request Body**: `CreateHabitDto`
- **Response**: `200 OK` with `HabitDto`

#### Get Habit by ID

- **Endpoint**: `GET /api/habits/{id}`
- **Description**: Retrieves a specific habit by its ID
- **Parameters**:
  - `id` (path, required): String identifier of the habit
- **Response**: `200 OK` with `HabitDto`

### Weather Forecast (Demo Endpoint)

#### Get Weather Forecast

- **Endpoint**: `GET /WeatherForecast`
- **Description**: Retrieves weather forecast data (demo endpoint)
- **Operation ID**: `GetWeatherForecast`
- **Response**: `200 OK` with array of `WeatherForecast`

## Data Models

### CreateHabitDto

Used for creating new habits.

**Required Properties:**

- `name` (string): Name of the habit
- `type` (HabitType): Type of habit (enumeration)
- `frequency` (FrequencyDto): How often the habit should be performed
- `target` (TargetDto): Target metrics for the habit

**Optional Properties:**

- `description` (string, nullable): Description of the habit
- `endDate` (string, date format, nullable): End date for the habit
- `milestone` (MilestoneDto, nullable): Milestone configuration

### HabitDto

Represents a complete habit with all tracking information.

**Properties:**

- `id` (string): Unique identifier
- `name` (string): Name of the habit
- `description` (string, nullable): Description
- `type` (HabitType): Type of habit
- `frequency` (FrequencyDto): Frequency configuration
- `target` (TargetDto): Target metrics
- `status` (HabitStatus): Current status
- `isArchived` (boolean): Whether the habit is archived
- `endDate` (string, date format, nullable): End date
- `milestone` (MilestoneDto, nullable): Milestone progress
- `createdAtUtc` (string, date-time): Creation timestamp
- `updatedAtUtc` (string, date-time, nullable): Last update timestamp
- `lastCompletedAtUtc` (string, date-time, nullable): Last completion timestamp

### HabitsCollectionDto

Container for multiple habits.

**Properties:**

- `data` (array of HabitDto): Array of habit objects

### FrequencyDto

Defines how often a habit should be performed.

**Required Properties:**

- `type` (FrequencyType): Type of frequency (enumeration)
- `timesPerPeriod` (integer): Number of times per period

### TargetDto

Defines the target metrics for a habit.

**Required Properties:**

- `value` (integer): Target value
- `unit` (string): Unit of measurement

### MilestoneDto

Tracks progress towards milestones (nullable).

**Properties:**

- `target` (integer): Target milestone value
- `current` (integer): Current progress value

### WeatherForecast

Demo model for weather data.

**Properties:**

- `date` (string, date format): Date of forecast
- `temperatureC` (integer): Temperature in Celsius
- `temperatureF` (integer): Temperature in Fahrenheit
- `summary` (string, nullable): Weather summary

## Enumerations

### HabitType

Integer enumeration representing different types of habits.

### HabitStatus

Integer enumeration representing the current status of a habit.

### FrequencyType

Integer enumeration representing different frequency types.

## Error Handling

The API uses standard HTTP status codes. Common responses include:

- `200 OK`: Request successful
- `400 Bad Request`: Invalid request data
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Rate Limiting

Currently, no rate limiting is implemented.

## Versioning

The API uses URL versioning with the current version being v1.

## Support

For questions or issues, please refer to the project documentation or contact the development team.
