# Copilot Instructions for DevHabits

## Project Overview

DevHabits is a .NET API for tracking and managing daily habits with progress monitoring, milestone tracking, and flexible frequency configuration.

## Coding Style & Standards

- Follow C# coding conventions and use PascalCase for public members.
- Use camelCase for private fields and local variables.
- Add XML documentation comments for all public APIs.
- Use nullable reference types where appropriate.
- Prefer `var` for local variables when the type is obvious.
- Use `async/await` for all I/O operations.

## Architecture Patterns

- Follow Clean Architecture principles.
- Use Entity Framework Core for data access.
- Implement DTOs for API request/response models.
- Use the repository pattern sparingly - prefer EF Core DbContext directly.
- Apply CQRS pattern for complex operations.

## API Development

- Use ASP.NET Core Web API controllers.
- Add proper OpenAPI documentation with XML comments.
- Include appropriate HTTP status codes in responses.
- Use `[Tags]` attributes for API grouping.
- Add `[ProducesResponseType]` attributes for better OpenAPI documentation.
- Validate input using Data Annotations or FluentValidation.

## Database & Entity Framework

- Use PostgreSQL as the primary database.
- Apply snake_case naming convention via EFCore.NamingConventions.
- Create proper database migrations for schema changes.
- Use GUID strings for entity IDs.
- Include audit fields (CreatedAt, UpdatedAt) on entities.

## Testing

- Write unit tests using xUnit.
- Use integration tests for API endpoints.
- Mock external dependencies using Moq or NSubstitute.
- Aim for high code coverage on business logic.

## Docker & Deployment

- Use Docker for containerization.
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
