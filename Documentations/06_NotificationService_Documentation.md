# NotificationService Documentation

## Responsibility

`NotificationService` is responsible for event-driven user notifications and notification read-state management.

## What It Does

- Consumes wallet top-up and transfer events.
- Creates persistent notification records for affected users.
- Returns notifications for authenticated users.
- Marks notifications as read.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| Framework | `ASP.NET Core Web API` |
| Data Access | `EF Core` |
| Database | `SQL Server` |
| Auth | `JWT Bearer` |
| Messaging | `RabbitMQ.Client` (consumers) |
| API Docs | `Swagger` |

## Runtime and Data

| Item | Value |
|---|---|
| Port | `5011` |
| Base Route | `/api/notification` |
| Database | `NotificationDB` |
| Table | `Notifications` |

## APIs and Responsible Methods

| Method | API | Auth | Service Method | Purpose |
|---|---|---|---|---|
| `GET` | `/api/notification` | User | `GetAllAsync` | Get notifications for current user |
| `PUT` | `/api/notification/{id}/read` | User | `MarkAsReadAsync` | Mark notification as read |
| Event | `WalletTopUpCompletedEvent` | Internal | `CreateAsync` | Create top-up notification |
| Event | `WalletTransferCompletedEvent` | Internal | `CreateAsync` | Create sender/receiver transfer notifications |

## Complete Workflow

1. Consumer listens on wallet-related queues.
2. Message is deserialized into event contract.
3. Service creates notification records in DB.
4. User fetches notifications through API.
5. User marks selected notification as read.

## Integrations

| Integration Type | Source | Target | Purpose |
|---|---|---|---|
| HTTP | Client/Gateway | `NotificationService` | Notification APIs |
| RabbitMQ | `WalletService` | `NotificationService` | Event-driven notification generation |

## Key Methods and Tasks

| Class | Method/Task |
|---|---|
| `NotificationController` | Expose notification APIs |
| `NotificationServiceImpl` | Notification retrieval/create/read logic |
| `WalletTopUpNotificationConsumer` | Consume top-up event |
| `WalletTransferNotificationConsumer` | Consume transfer event |
| `NotificationRepository` | Notification data access |
| `NotificationDbContext` | Entity mapping |

## Key Files

- `NotificationService/Controllers/NotificationController.cs`
- `NotificationService/Services/NotificationServiceImpl.cs`
- `NotificationService/Consumers/WalletTopUpNotificationConsumer.cs`
- `NotificationService/Consumers/WalletTransferNotificationConsumer.cs`
- `NotificationService/Repositories/NotificationRepository.cs`
- `NotificationService/Data/NotificationDbContext.cs`
- `NotificationService/DTOs/NotificationDTOs.cs`
