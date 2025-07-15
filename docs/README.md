# DevHabits API Documentation

Welcome to the DevHabits API documentation. This documentation provides comprehensive information about the DevHabits API, a RESTful web service for tracking and managing daily habits.

## 📖 Documentation Contents

### 1. [API Documentation](./api-documentation.md)
Complete API reference with detailed information about all endpoints, request/response formats, and data models.

### 2. [Quick Start Guide](./quick-start-guide.md)
Get up and running quickly with the DevHabits API. Includes common use cases and sample code in multiple programming languages.

### 3. [Data Models Reference](./data-models.md)
Detailed documentation of all data models, including request/response DTOs, nested objects, and enumerations.

### 4. [OpenAPI Specification](./openapi-spec.json)
Machine-readable OpenAPI 3.0.1 specification file that can be used with tools like Swagger UI, Postman, or code generators.

## 🚀 Getting Started

### Base URL
```
https://localhost:5002/
```

### OpenAPI Specification URL
```
https://localhost:5002/openapi/v1.json
```

### Available Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/habits` | Get all habits |
| POST | `/api/habits` | Create a new habit |
| GET | `/api/habits/{id}` | Get a specific habit by ID |
| GET | `/WeatherForecast` | Get weather forecast (demo endpoint) |

## 🔧 Tools and Resources

### Postman Collection
Ready-to-use Postman collection is available at:
- [DevHabit.postman_collection.json](../PostmanCollections/DevHabit.postman_collection.json)

### OpenAPI Tools
You can use the OpenAPI specification with various tools:
- **Swagger UI**: For interactive API exploration
- **Postman**: Import the OpenAPI spec directly
- **Code Generators**: Generate client libraries for various languages
- **API Testing Tools**: For automated testing

## 📊 API Overview

### Supported Operations
- **Habit Management**: Create, read, and manage habits
- **Progress Tracking**: Track habit completion and milestones
- **Flexible Targeting**: Set custom targets with different units
- **Frequency Configuration**: Support for daily, weekly, monthly, and custom frequencies

### Content Types
The API supports multiple content types:
- **JSON** (recommended): `application/json`
- **XML**: `application/xml`
- **Plain Text**: `text/plain`

### Response Format
All responses follow a consistent structure with appropriate HTTP status codes and content-type headers.

## 🏗️ Architecture

The DevHabits API is built using:
- **Framework**: ASP.NET Core
- **OpenAPI Version**: 3.0.1
- **Data Format**: JSON (primary), XML (supported)
- **Authentication**: Currently none (may be added in future versions)

## 📝 Example Usage

### Create a New Habit
```bash
curl -X POST "https://localhost:5002/api/habits" \
  -H "Content-Type: application/json" \
  -d '{
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
    }
  }'
```

### Get All Habits
```bash
curl -X GET "https://localhost:5002/api/habits" \
  -H "Accept: application/json"
```

## 🔍 Additional Resources

### SQL Scripts
Database initialization scripts are available in the `SqlScripts` folder:
- [habits.sql](../SqlScripts/habits.sql)

### Development Setup
For development setup and Docker configuration, check:
- [docker-compose.yml](../docker-compose.yml)
- [docker-compose.override.yml](../docker-compose.override.yml)

## 🤝 Contributing

This is an open-source project. Feel free to:
- Report issues
- Submit feature requests
- Contribute code improvements
- Improve documentation

## 📄 License

This project is licensed under the terms specified in the [LICENSE](../LICENSE) file.

---

**Last Updated**: July 15, 2025  
**API Version**: v1.0.0  
**Documentation Version**: 1.0.0
