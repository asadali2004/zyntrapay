# AuthService Documentation

## Responsibility

`AuthService` is responsible for identity management: user/admin registration, login, JWT issuance, and admin-side user status management.

## What It Does

- Registers users and admins.
- Validates uniqueness of email/phone.
- Hashes passwords using `BCrypt`.
- Issues JWT tokens after login.
- Exposes admin endpoints for user listing and status toggle.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| Framework | `ASP.NET Core Web API` |
| Data Access | `EF Core` |
| Database | `SQL Server` |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Password Hashing | `BCrypt.Net-Next` |
| API Docs | `Swagger` |

## Runtime and Data

| Item | Value |
|---|---|
| Port | `5003` |
| Base Route | `/api/auth` |
| Database | `AuthDB` |
| Main Table | `Users` |

## APIs and Responsible Methods

| Method | API | Auth | Service Method | Purpose |
|---|---|---|---|---|
| `POST` | `/api/auth/register` | Public | `RegisterAsync` | Register user |
| `POST` | `/api/auth/register-admin` | Public (secret key validated) | `RegisterAdminAsync` | Register admin |
| `POST` | `/api/auth/login` | Public | `LoginAsync` | Authenticate and issue JWT |
| `GET` | `/api/auth/admin/users` | Admin | `GetAllUsersAsync` | List users |
| `PUT` | `/api/auth/admin/users/{id}/toggle` | Admin | `ToggleUserStatusAsync` | Activate/deactivate user |

## Complete Workflow

1. Registration request arrives.
2. Service checks uniqueness constraints.
3. Password is hashed and stored.
4. Login validates credentials and active status.
5. JWT is generated with id/email/role claims.
6. Admin-only endpoints operate under role authorization.

## Integrations

| Integration Type | Source | Target | Purpose |
|---|---|---|---|
| HTTP | Client/Gateway | `AuthService` | Auth and admin user operations |
| HTTP | `AdminService` | `AuthService` | Get users and toggle status |

## Key Methods and Tasks

| Class | Method/Task |
|---|---|
| `AuthController` | Handle API requests |
| `AuthServiceImpl` | Business logic for register/login/admin operations |
| `AuthDbContext` | User entity and DB mapping |
| `ServiceExtensions` | DI, JWT, DB, Swagger configuration |

## Key Files

- `AuthService/Controllers/AuthController.cs`
- `AuthService/Services/AuthServiceImpl.cs`
- `AuthService/Repositories/AuthRepository.cs`
- `AuthService/Data/AuthDbContext.cs`
- `AuthService/Extensions/ServiceExtensions.cs`
- `AuthService/DTOs/AuthDTOs.cs`
