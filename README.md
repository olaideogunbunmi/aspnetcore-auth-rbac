# ASP.NET Core Role-Based Authentication API

A RESTful Web API built with ASP.NET Core 8 and ASP.NET Core Identity that provides JWT-based authentication with role-based and claims-based authorization.

## Features

- User Management - create, update, enable/disable user accounts
- Role Management - full CRUD for roles
- Claims Management - assign/remove claims on users
- JWT Access Token Generation
- Logout
- Forgot Password and Password Reset
- Change Password
- Email delivery for password reset using MailKit
- Mailtrap Sandbox integration for testing password reset emails
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
- Refresh token support with rotation, revocation, and reuse detection

## Tech Stack

- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core
- JWT Bearer Authentication
- AutoMapper
- Newtonsoft.Json / JSON Patch
- Swagger/OpenAPI
- SQL Server
- MailKit for SMTP email delivery
- Mailtrap for email testing and sandboxing
- ASP.NET Core `ILogger` for logging

## Architecture and Design Decisions

This API follows a set of conventions applied consistently across the codebase.
- RFC 9457 Problem Details via `AddProblemDetails()` and `UseExceptionHandler()` for all error responses
- Result objects with failure-reason enums (e.g. `UpdateUserResult`, `AssignRoleResult`) instead of throwing exceptions for expected failure paths
- Input/output DTO separation — request and response models are never the same type
- `ProjectTo<>` for efficient, read-only projection queries
- `AsNoTracking()` scoped strictly to read-only operations
- Sequence-based PublicId to avoid exposing internal database primary keys
- `UpdateSecurityStampAsync()` to invalidate previously issued JWTs when account state changes (e.g. account disable)
- ASP.NET Core `ILogger` for logging security-related events such as refresh-token reuse detection

## Authentication Flow

1. User registers through the registration endpoint.
2. ASP.NET Core Identity manages the user account and password hashing.
3. User logs in with valid credentials.
4. The API issues a short-lived JWT access token and refresh token.
5. The client uses the access token to access protected resources.
6. When the access token expires, the refresh token can be used to obtain a new access token.
7. Refresh token rotation replaces the previous refresh token.
8. Revoked or reused refresh tokens are rejected.
9. A user can log out, which revokes their refresh tokens.
10. A user who forgets their password can request a password reset.
11. The API generates a password reset token and sends a password reset email using MailKit.
12. During development, Mailtrap provides a sandbox for receiving and inspecting password reset emails.
13. The user can use the reset token to set a new password.
14. An authenticated user can change their password through the change-password endpoint.

## Getting Started

### Prerequisites

Before running the project, ensure that you have the following installed or available:
- .NET 8 SDK
- SQL Server
- Git
- An API testing tool such as Postman
- A Mailtrap account for testing password reset emails


### Installation

``` bash
# clone the repository
git clone https://github.com/olaideogunbunmi/aspnetcore-auth-rbac.git

# navigate to the project directory
cd aspnetcore-auth-rbac

# restore dependencies
dotnet restore

# build the project
dotnet build
```

### Configuration

The project uses ASP.NET Core User Secrets to store sensitive configuration values outside of source control. Configuration is loaded through ASP.NET Core's standard configuration system, including `appsettings.json`, `appsettings.Development.json` (environment-specific configuration), and User Secrets. During development, User Secrets stores the sensitive configuration in a `secrets.json` file outside the project directory. This project requires a database connection string.

You can configure this using **Option A (Recommended for Security)** or **Option B (Quickest Setup)**.

Sensitive values include:
- SQL Server connection string
- JWT secret key
- SMTP username and password

### Required Settings

The following configuration settings are required for the application to run correctly:

| Key                                   | Description                                           | Where to set it                                                                               |
| :--------------------                 | :------------------------------------------           | :-------------------------------------------------------------------------------------------  |
| `JWT:Issuer`                          | Token Issuer URL                                      | `appsettings.json`                                                                            |
| `JWT:Audience`                        | Token Audience URL                                    | `appsettings.json`                                                                            |
| `JWT:Key`                             | Secret key used to sign tokens. Minimum of 32 chars   | `User Secrets` or `appsettings.Development.json` for local development                        |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string                          | `User Secrets` or `appsettings.Development.json` for local development                        |
| `Smtp:Host`                           | SMTP server host                                      | `User Secrets`                                                                                |
| `Smtp:Port`                           | SMTP server port                                      | `User Secrets`                                                                                |
| `Smtp:Username`                       | SMTP username                                         | `User Secrets`                                                                                |
| `Smtp:Password`                       | SMTP password                                         | `User Secrets`                                                                                |
| `Smtp:FromAddress`                    | Sender email address                                  | `appsettings.Development.json`                                                                |
| `Smtp:FromName`                       | Sender display name                                   | `appsettings.Development.json`                                                                |


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
4. Set your connection string:
``` bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SQL_SERVER_INSTANCE;Database=RoleBasedAuthenticationDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

#### Option B: Using `appsettings.Development.json` for local development (Quick Setup)

If you prefer not to use User Secrets, you can configure your connection string directly into the configuration file.

1. Open `appsettings.Development.json`, locate the `ConnectionStrings` section, replace the placeholder with your local database details
``` json
"ConnectionStrings": {
        "DefaultConnection": "Server=YOUR_SQL_SERVER_INSTANCE;Database=RoleBasedAuthenticationDB;Trusted_Connection=True;TrustServerCertificate=True;"
    }
```
2. For JWT key configuration:
Locate `JWT` section and replace the placeholder below with your actual secret key
``` json
"JWT": {
    "Key": "YOUR_SECRET_KEY"
}
```

**Warning:** If you choose Option B, do not commit your actual connection string, credentials or secret key to GitHub


### Database Setup

Run the following command from the project directory:
``` bash
dotnet ef database update
```
### Run the Application
``` bash
dotnet run
```

### Email Configuration

The application uses MailKit to send password reset emails. During development, Mailtrap is used as a sandbox email service to capture and inspect outgoing
emails without sending them to real users.

Mailtrap SMTP host, port, username and password are stored securely using ASP.NET Core User Secrets and are not committed to source control. The sender address and sender name can be configured in `appsettings.Development.json`. 

1. `appsettings.Development.json` configuration
 ``` json
  "Smtp": {
    "FromAddress": "YOUR_EMAIL_ADDRESS",
    "FromName": "YOUR_DISPLAY_NAME"
  }
```
2. User Secrets: Store the Mailtrap SMTP connection settings and credentials using User Secrets:
``` bash
dotnet user-secrets set "Smtp:Host" "YOUR_MAILTRAP_HOST"
dotnet user-secrets set "Smtp:Port" "YOUR_MAILTRAP_PORT"
dotnet user-secrets set "Smtp:Username" "YOUR_MAILTRAP_USERNAME"
dotnet user-secrets set "Smtp:Password" "YOUR_MAILTRAP_PASSWORD"
```

You can obtain the SMTP host, port, username, and password from your Mailtrap account.

The API will be available at the URL (port) configured in `launchSettings.json`. Once running, Swagger UI is available at `https://localhost:{port}/swagger` for exploring and testing the endpoints directly in the browser.

## API Overview

| Resource                      | Description                                         |
|-------------------------      |-------------------------------------------------    |
| `/api/auth`                   | Registration, login, logout, token issuance         |
| `/api/auth/forgot-password`   | Request a password reset                            |
| `/api/auth/reset-password`    | Reset password using a reset token through email    | 
| `/api/auth/change-password`   | Change password for authenticated users             |
| `/api/auth/refresh`           | Refresh access token                                |
| `/api/users`                  | User CRUD, enable/disable                           |
| `/api/users/{id}/claims`      | Claims management                                   |
| `/api/roles`                  | Role CRUD                                           |
| `/api/profile`                | Authenticated user's own profile                    |


## Project Structure
```
RoleBasedAuthenticationApi/
├── Properties/
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
└── appsettings.Development.json
```





