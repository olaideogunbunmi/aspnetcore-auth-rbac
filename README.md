# ASP.NET Core Role-Based Authentication API

A WEB API built with ASP.NET Core 8, ASP.NET Core Identity that provides JWT-based authentication with role and claims-based authorization support.

## Features
- User Management - create, update, enable/disable user accounts
- Role Management - full CRUD for roles
- Claims Management - assign/remove claims on users
- JWT Acces Token Generation
- Profile Endpoint - authenticated user profile retrieval
- RFC 9457 Problem Details — standardized error responses across the API
- Separation of Concerns - service layers, result objects and input and output DTOs
- ASP.NET Core Identity integration
- Entity Framework Core database persistence
- SQL Server database integration
- AutoMapper for object mapping
- JSON Patch support for partial updates
- Swagger/OpenAPI API documentation
- Secure configuration using ASP.NET Core User Secrets
- Global JSON null-value handling

## Tech Stack
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core
- JWT Bearer Authentication

## Architecture and Design Decisions
This API follows a set of conventions applied consistently across the codebase.
- RFC 9457 Problem Details via `AddProblemDetails()` and `UseExceptionHandler()` for all error responses
- Result objects with failure-reason enums (e.g. `UpdateUserResult`, `AssignRoleResult`) instead of throwing exceptions for expected failure paths
- Input/output DTO separation — request and response models are never the same type
- `ProjectTo<>` for efficient, read-only projection queries
- `AsNoTracking()` scoped strictly to read-only operations
- Sequence-based PublicId to avoid exposing internal database primary keys
- `UpdateSecurityStampAsync()` to invalidate previously issued JWTs when account state changes (e.g. account disable)

## Getting Started

### Prerequisites
efore running the project, ensure that you have the following installed:
- .NET 8 SDK
- SQL Server
- Git
- An API testing tool such POSTMAN

### Installation
```
# clone the repository
git clone https://github.com/olaideogunbunmi/aspnetcore-auth-rbac.git

# navigate to the project directory
cd aspnetcore-auth-rbac

# restore dependencies
dotnet restore
```