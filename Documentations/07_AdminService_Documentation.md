# AdminService Documentation

## Responsibility

`AdminService` is responsible for admin-only orchestration: KYC moderation, user management, dashboard aggregation, and admin audit logging.

## What It Does

- Exposes admin APIs under `/api/admin`.
- Calls `UserService` for KYC operations.
- Calls `AuthService` for user operations.
- Forwards incoming JWT token to downstream services.
- Stores admin actions in `AdminActions`.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| Framework | `ASP.NET Core Web API` |
| Data Access | `EF Core` |
| Database | `SQL Server` |
| Auth | `JWT Bearer` + role-based authorization |
| Service-to-Service HTTP | `HttpClientFactory` |
| Token Forwarding | `DelegatingHandler` (`AuthTokenHandler`) |
| API Docs | `Swagger` |

## Runtime and Data

| Item | Value |
|---|---|
| Port | `5013` |
| Base Route | `/api/admin` |
| Database | `AdminDB` |
| Table | `AdminActions` |

## APIs and Responsible Methods

| Method | API | Auth | Service Method | Purpose |
|---|---|---|---|---|
| `GET` | `/api/admin/kyc/pending` | Admin | `GetPendingKycsAsync` | Get pending KYC list |
| `PUT` | `/api/admin/kyc/{kycId}/review` | Admin | `ReviewKycAsync` | Approve/reject KYC and audit |
| `GET` | `/api/admin/users` | Admin | `GetAllUsersAsync` | Get all users |
| `PUT` | `/api/admin/users/{userId}/toggle` | Admin | `ToggleUserStatusAsync` | Toggle user status and audit |
| `GET` | `/api/admin/dashboard` | Admin | `GetDashboardAsync` | Get dashboard stats |

## Complete Workflow

1. Admin sends authenticated request to admin API.
2. `AdminService` validates admin access via role claim.
3. For cross-service tasks, it calls downstream services using typed `HttpClient`.
4. `AuthTokenHandler` forwards bearer token to downstream request.
5. Result is returned to caller.
6. Critical operations are logged in `AdminActions`.

## Integrations

| Integration Type | Source | Target | Purpose |
|---|---|---|---|
| HTTP | Client/Gateway | `AdminService` | Admin APIs |
| HTTP | `AdminService` | `UserService` | KYC retrieval/review |
| HTTP | `AdminService` | `AuthService` | User list/status operations |

## Key Methods and Tasks

| Class | Method/Task |
|---|---|
| `AdminController` | Expose admin endpoints |
| `AdminServiceImpl` | Orchestrate admin business flows |
| `UserServiceClient` | Call `UserService` APIs |
| `AuthServiceClient` | Call `AuthService` APIs |
| `AuthTokenHandler` | Forward bearer token |
| `AdminRepository` | Persist admin audit records |
| `AdminDbContext` | Entity mapping |

## Key Files

- `AdminService/Controllers/AdminController.cs`
- `AdminService/Services/AdminServiceImpl.cs`
- `AdminService/Services/UserServiceClient.cs`
- `AdminService/Services/AuthServiceClient.cs`
- `AdminService/Services/AuthTokenHandler.cs`
- `AdminService/Repositories/AdminRepository.cs`
- `AdminService/Data/AdminDbContext.cs`
- `AdminService/DTOs/AdminDTOs.cs`
