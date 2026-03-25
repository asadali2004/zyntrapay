# UserService Documentation

## Responsibility

`UserService` is responsible for user profile management and KYC lifecycle management.

## What It Does

- Creates and fetches user profile.
- Accepts and tracks KYC submission.
- Exposes admin-only KYC review operations.
- Enforces one profile and one KYC submission per user.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| Framework | `ASP.NET Core Web API` |
| Data Access | `EF Core` |
| Database | `SQL Server` |
| Auth | `JWT Bearer` |
| API Docs | `Swagger` |

## Runtime and Data

| Item | Value |
|---|---|
| Port | `5005` |
| Base Route | `/api/user` |
| Database | `UserDB` |
| Tables | `UserProfiles`, `KycSubmissions` |

## APIs and Responsible Methods

| Method | API | Auth | Service Method | Purpose |
|---|---|---|---|---|
| `POST` | `/api/user/profile` | User | `CreateProfileAsync` | Create profile |
| `GET` | `/api/user/profile` | User | `GetProfileAsync` | Get own profile |
| `POST` | `/api/user/kyc` | User | `SubmitKycAsync` | Submit KYC |
| `GET` | `/api/user/kyc` | User | `GetKycStatusAsync` | Get KYC status |
| `GET` | `/api/user/admin/kyc/pending` | Admin | `GetPendingKycsAsync` | List pending KYC |
| `PUT` | `/api/user/admin/kyc/{kycId}/review` | Admin | `ReviewKycAsync` | Approve/reject KYC |

## Complete Workflow

1. User sends authenticated profile/KYC request.
2. Controller extracts `AuthUserId` from JWT.
3. Service performs validation rules.
4. Repository persists or fetches data.
5. Response DTO is returned to caller.
6. Admin endpoints allow KYC moderation with role restrictions.

## Integrations

| Integration Type | Source | Target | Purpose |
|---|---|---|---|
| HTTP | Client/Gateway | `UserService` | Profile and KYC APIs |
| HTTP | `AdminService` | `UserService` | Pending KYC and review actions |

## Key Methods and Tasks

| Class | Method/Task |
|---|---|
| `UserController` | Extract auth user id and expose APIs |
| `UserServiceImpl` | Profile/KYC business rules |
| `UserRepository` | Data access for profile/KYC |
| `UserDbContext` | Entity mapping and DB config |

## Key Files

- `UserService/Controllers/UserController.cs`
- `UserService/Services/UserServiceImpl.cs`
- `UserService/Repositories/UserRepository.cs`
- `UserService/Data/UserDbContext.cs`
- `UserService/DTOs/UserDTOs.cs`
- `UserService/Models/UserProfile.cs`
- `UserService/Models/KycSubmission.cs`
