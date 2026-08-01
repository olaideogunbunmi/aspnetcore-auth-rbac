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
Before running the project, ensure that you have the following installed:
- .NET 8 SDK
- SQL Server
- Git
- An API testing tool such POSTMAN


### Installation
``` bash
# clone the repository
git clone https://github.com/olaideogunbunmi/aspnetcore-auth-rbac.git

# navigate to the project directory
cd aspnetcore-auth-rbac

# restore dependencies
dotnet restore
```

### Configuration
The project uses ASP.NET Core User Secrets to store sensitive configuration values outside of source control, and it uses the standard ASP.NET Core configuration hierarchy: 
`appsettings.json` → `appsettings.Development.json` → `secrets.json`.

This project requires a database connection string. You can configure this using **Option A (Recommended for Security)** or **Option B (Quickest Setup)**.

Sensitive values include
- SQL Server connection string
- JWT secret key

### Required Settings
The following must be set for JWT Authentication to work:

| Key | Description | Where to set it
| :--- | :----------- | :-------------- |
| `JWT:Issuer` | Token Issuer URL | `appsettings.json` |
| `JWT:Audience` | Token Audience URL | `appsettings.json`
| `JWT:Key` | Secret key used to sign tokens. Minimum of 32 chars | `User Secrets` |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | `appsettings.Development.json` or `User Secrets` |


#### Option A: Using .NET User Secrets (Recommended)

This approach keeps your database credentials safely stored outside of the project directory, preventing accidental commits to source control.
1. Open your terminal in the project root directory.

2. Initialize user secrets:
``` bash
dotnet user-secrets init
```
3. Set your JWT key:
``` bash
dotnet user-secrets set "JWT:Key" "generate-a-very-long-random-key-here"
```
4. Set your connection string
``` bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SQL_SERVER_INSTANCE;Database=RoleBasedAuthenticationDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Option B: Using appsettings.json (Quick Setup)

If you prefer not to use user secrets, you can paste your connection string directly into the configuration file.
1. Open `appsettings.Development.json` or (`appsettings.json`)
1. Locate the `ConnectionStrings` section, replace the placeholder with your local database details
``` json
"ConnectionStrings": {
        "DefaultConnection": "Server=YOUR_SQL_SERVER_INSTANCE;Database=RoleBasedAuthenticationDB;Trusted_Connection=True;TrustServerCertificate=True;"
    }
```
For Jwt Key configuration
1. Open `appsettings.json`
1. Locate `JWT` section, mirror the code below
``` json
"JWT": {
    "Key": "YOUR_SECRET_KEY"
}
```

**Warning:** If you choose Option B, be careful not to commit your actual connection string or secret key to GitHub


### Database Setup
Run the following command from the project directory:
``` bash
dotnet ef database update
```
### Run the Application
``` bash
dotnet run
```

The API will be available at the port configured in `launchSettings.json`. Once running, Swagger UI is available at `https://localhost:{port}/swagger` for exploring and testing the endpoints directly in the browser.

## API Overview

| Resource               | Description                          |
|------------------------|--------------------------------------|
| /api/auth              | Registration, login, token issuance  |
| /api/users             | User CRUD, enable/disable            |
| /api/users/{id}/claims | Claims management                    |
| /api/roles             | Role CRUD                            |
| /api/profile           | Authenticated user's own profile     |


## Project Structure
```
RoleBasedAuthenticationApi/
├── Configuration/
├── Controllers/
├── Data/
├── DTO/
├── Interfaces/
├── Migrations/
├── Models/
├── Services/
├── Program.cs
├── appsettings.json
```





