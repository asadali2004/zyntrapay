# ZyntraPay — Overall System Workflow

## Responsibility

The system provides a complete digital wallet and rewards platform using microservices. It is responsible for identity, profile/KYC, wallet operations, rewards, notifications, and admin governance.

## What It Does

- Accepts all client traffic through `ApiGateway`.
- Authenticates users with JWT.
- Handles profile/KYC, wallet, rewards, and notifications via dedicated services.
- Supports admin workflows for KYC moderation and user status control.
- Uses RabbitMQ for async post-processing.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| API Framework | `ASP.NET Core Web API` |
| Gateway | `Ocelot` |
| Database | `SQL Server` + `EF Core` |
| Auth | `JWT Bearer` |
| Messaging | `RabbitMQ` |
| API Documentation | `Swagger` |
| Password Hashing | `BCrypt` |

## Runtime and Data Ownership

| Component | Port | Responsibility | Database |
|---|---:|---|---|
| `ApiGateway` | 5001 | Route forwarding | N/A |
| `AuthService` | 5003 | Register/login/user status | `AuthDB` |
| `UserService` | 5005 | Profile + KYC | `UserDB` |
| `WalletService` | 5007 | Wallet + ledger + transfers | `WalletDB` |
| `RewardsService` | 5009 | Points + tier + redemption | `RewardsDB` |
| `NotificationService` | 5011 | Notification storage/read state | `NotificationDB` |
| `AdminService` | 5013 | Admin orchestration + audit | `AdminDB` |

## APIs and Responsible Services

| Gateway API Prefix | Downstream Service | Purpose |
|---|---|---|
| `/gateway/auth/*` | `AuthService` | Authentication and user identity |
| `/gateway/user/*` | `UserService` | Profile and KYC |
| `/gateway/wallet/*` | `WalletService` | Wallet operations |
| `/gateway/rewards/*` | `RewardsService` | Rewards and redemption |
| `/gateway/notification/*` | `NotificationService` | User notifications |
| `/gateway/admin/*` | `AdminService` | Admin-only operations |

## Complete Workflow

1. User registers/logs in (`AuthService`) and gets JWT.
2. User creates profile/KYC (`UserService`).
3. User creates wallet/top-up/transfer (`WalletService`).
4. `WalletService` publishes events:
   - `WalletTopUpCompletedEvent`
   - `WalletTransferCompletedEvent`
5. `RewardsService` consumes top-up events and awards points.
6. `NotificationService` consumes events and creates user notifications.
7. Admin performs moderation/management (`AdminService`), and actions are audited.

## Integrations

| Integration Type | Source | Target | Usage |
|---|---|---|---|
| HTTP | Client | `ApiGateway` | Public entry |
| HTTP | `AdminService` | `UserService` | KYC list/review |
| HTTP | `AdminService` | `AuthService` | User list/toggle |
| RabbitMQ | `WalletService` | `RewardsService` | Points awarding |
| RabbitMQ | `WalletService` | `NotificationService` | Notification generation |

## Key Files

- `ApiGateway/ocelot.json`
- `*/Program.cs`
- `*/Extensions/ServiceExtensions.cs`
- `Shared.Events/*`
- `Database/*.sql`
- `README.md`
