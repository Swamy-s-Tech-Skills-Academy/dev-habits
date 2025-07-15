# DevHabits API - Quick Start Guide

## Getting Started

This guide will help you quickly get started with the DevHabits API.

### Base URL
```
https://localhost:5002/
```

### OpenAPI Specification
The OpenAPI specification is available at:
```
https://localhost:5002/openapi/v1.json
```

## Common Use Cases

### 1. Retrieve All Habits

**Request:**
```http
GET /api/habits
Accept: application/json
```

**Response:**
```json
{
  "data": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "type": 0,
      "frequency": {
        "type": 0,
        "timesPerPeriod": 0
      },
      "target": {
        "value": 0,
        "unit": "string"
      },
      "status": 0,
      "isArchived": false,
      "endDate": "2025-07-15",
      "milestone": {
        "target": 0,
        "current": 0
      },
      "createdAtUtc": "2025-07-15T12:00:00Z",
      "updatedAtUtc": "2025-07-15T12:00:00Z",
      "lastCompletedAtUtc": "2025-07-15T12:00:00Z"
    }
  ]
}
```

### 2. Create a New Habit

**Request:**
```http
POST /api/habits
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Daily Exercise",
  "description": "Exercise for 30 minutes daily",
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

**Response:**
```json
{
  "id": "generated-id",
  "name": "Daily Exercise",
  "description": "Exercise for 30 minutes daily",
  "type": 1,
  "frequency": {
    "type": 1,
    "timesPerPeriod": 1
  },
  "target": {
    "value": 30,
    "unit": "minutes"
  },
  "status": 0,
  "isArchived": false,
  "endDate": "2025-12-31",
  "milestone": {
    "target": 100,
    "current": 0
  },
  "createdAtUtc": "2025-07-15T12:00:00Z",
  "updatedAtUtc": null,
  "lastCompletedAtUtc": null
}
```

### 3. Get Specific Habit

**Request:**
```http
GET /api/habits/{id}
Accept: application/json
```

**Response:**
```json
{
  "id": "specific-habit-id",
  "name": "Daily Exercise",
  "description": "Exercise for 30 minutes daily",
  "type": 1,
  "frequency": {
    "type": 1,
    "timesPerPeriod": 1
  },
  "target": {
    "value": 30,
    "unit": "minutes"
  },
  "status": 0,
  "isArchived": false,
  "endDate": "2025-12-31",
  "milestone": {
    "target": 100,
    "current": 25
  },
  "createdAtUtc": "2025-07-15T12:00:00Z",
  "updatedAtUtc": "2025-07-15T14:00:00Z",
  "lastCompletedAtUtc": "2025-07-15T08:00:00Z"
}
```

## Sample Code

### JavaScript/Node.js

```javascript
const BASE_URL = 'https://localhost:5002';

// Get all habits
async function getAllHabits() {
  const response = await fetch(`${BASE_URL}/api/habits`, {
    method: 'GET',
    headers: {
      'Accept': 'application/json'
    }
  });
  return await response.json();
}

// Create a new habit
async function createHabit(habitData) {
  const response = await fetch(`${BASE_URL}/api/habits`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    },
    body: JSON.stringify(habitData)
  });
  return await response.json();
}

// Get habit by ID
async function getHabitById(id) {
  const response = await fetch(`${BASE_URL}/api/habits/${id}`, {
    method: 'GET',
    headers: {
      'Accept': 'application/json'
    }
  });
  return await response.json();
}
```

### C#

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class DevHabitsApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://localhost:5002";

    public DevHabitsApiClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<HabitsCollectionDto> GetAllHabitsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/api/habits");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<HabitsCollectionDto>(json);
    }

    public async Task<HabitDto> CreateHabitAsync(CreateHabitDto habitData)
    {
        var json = JsonSerializer.Serialize(habitData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/api/habits", content);
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<HabitDto>(responseJson);
    }

    public async Task<HabitDto> GetHabitByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/api/habits/{id}");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<HabitDto>(json);
    }
}
```

### Python

```python
import requests
import json

BASE_URL = "https://localhost:5002"

def get_all_habits():
    response = requests.get(f"{BASE_URL}/api/habits", 
                          headers={"Accept": "application/json"})
    return response.json()

def create_habit(habit_data):
    response = requests.post(f"{BASE_URL}/api/habits", 
                           json=habit_data,
                           headers={"Content-Type": "application/json", 
                                  "Accept": "application/json"})
    return response.json()

def get_habit_by_id(habit_id):
    response = requests.get(f"{BASE_URL}/api/habits/{habit_id}", 
                          headers={"Accept": "application/json"})
    return response.json()

# Example usage
new_habit = {
    "name": "Daily Reading",
    "description": "Read for 30 minutes daily",
    "type": 1,
    "frequency": {
        "type": 1,
        "timesPerPeriod": 1
    },
    "target": {
        "value": 30,
        "unit": "minutes"
    }
}

created_habit = create_habit(new_habit)
print(f"Created habit: {created_habit}")
```

## Error Handling

The API uses standard HTTP status codes:

- **200 OK**: Request successful
- **400 Bad Request**: Invalid request data
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

Always check the response status code and handle errors appropriately in your application.

## Next Steps

1. Review the complete [API Documentation](./api-documentation.md)
2. Examine the [OpenAPI Specification](./openapi-spec.json)
3. Test the API using tools like Postman or curl
4. Check out the [Postman Collection](../PostmanCollections/DevHabit.postman_collection.json) for ready-to-use requests
