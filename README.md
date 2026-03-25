# ZyntraPay — Digital Wallet & Rewards System

A microservices-based digital wallet and loyalty platform built with `.NET 8`, `SQL Server`, `RabbitMQ`, and `Ocelot API Gateway`.

## Tech Stack

- Backend: `.NET 8 Web API` (Microservices)
- Database: `SQL Server` + `EF Core`
- Auth: `JWT Bearer Tokens`
- Messaging: `RabbitMQ`
- Gateway: `Ocelot`
- Frontend: `Angular` (planned/in progress)

## Services & Ports

| Service | Port |
|---|---|
| API Gateway | 5001 |
| AuthService | 5003 |
| UserService | 5005 |
| WalletService | 5007 |
| RewardsService | 5009 |
| NotificationService | 5011 |
| AdminService | 5013 |

## Architecture Overview

`Angular -> API Gateway -> Microservices`

Gateway routes:

- `/gateway/auth/*` -> `AuthService`
- `/gateway/user/*` -> `UserService`
- `/gateway/wallet/*` -> `WalletService`
- `/gateway/rewards/*` -> `RewardsService`
- `/gateway/notification/*` -> `NotificationService`
- `/gateway/admin/*` -> `AdminService`

## Event Flow (RabbitMQ)

Implemented:

- `WalletService` publishes `WalletTopUpCompletedEvent`
  - consumed by `RewardsService` (points awarding)
  - consumed by `NotificationService` (top-up notification)
- `WalletService` publishes `WalletTransferCompletedEvent`
  - consumed by `NotificationService` (sender/receiver notification)

Shared contracts are in `Shared.Events/`.

## Database Setup

Run SQL scripts from `Database/` in order:

1. `01_AuthDB.sql`
2. `02_UserDB.sql`
3. `03_WalletDB.sql`
4. `04_RewardsDB.sql`
5. `05_NotificationDB.sql`
6. `06_AdminDB.sql`

> Note: Services use EF Core migrations and auto-migrate on startup. SQL scripts are schema references.

## Run the Project

1. Update connection strings in each service `appsettings.json`
2. Ensure RabbitMQ is running
3. In Visual Studio, set multiple startup projects for all services
4. Start debugging (`F5`)

## Admin Registration

Use `AuthService` admin secret from `AuthService/appsettings.json`:

`POST /gateway/auth/register-admin`

```json
{
  "email": "admin@digitalwallet.com",
  "phoneNumber": "9000000001",
  "password": "Admin@123",
  "adminSecretKey": "AdminSecret@2026"
}
```

## Swagger URLs

- `https://localhost:5003/swagger` (`AuthService`)
- `https://localhost:5005/swagger` (`UserService`)
- `https://localhost:5007/swagger` (`WalletService`)
- `https://localhost:5009/swagger` (`RewardsService`)
- `https://localhost:5011/swagger` (`NotificationService`)
- `https://localhost:5013/swagger` (`AdminService`)

## API Summary

### AuthService

- `POST /gateway/auth/register`
- `POST /gateway/auth/register-admin`
- `POST /gateway/auth/login`
- `GET /gateway/auth/admin/users` (Admin)
- `PUT /gateway/auth/admin/users/{id}/toggle` (Admin)

### UserService

- `POST /gateway/user/profile`
- `GET /gateway/user/profile`
- `POST /gateway/user/kyc`
- `GET /gateway/user/kyc`
- `GET /gateway/user/admin/kyc/pending` (Admin)
- `PUT /gateway/user/admin/kyc/{kycId}/review` (Admin)

### WalletService

- `POST /gateway/wallet/create`
- `GET /gateway/wallet/balance`
- `POST /gateway/wallet/topup`
- `POST /gateway/wallet/transfer`
- `GET /gateway/wallet/transactions`
- `GET /gateway/wallet/transactions/{id}`

### RewardsService

- `GET /gateway/rewards/summary`
- `GET /gateway/rewards/catalog`
- `POST /gateway/rewards/redeem`
- `GET /gateway/rewards/history`

### NotificationService

- `GET /gateway/notification`
- `PUT /gateway/notification/{id}/read`

### AdminService

- `GET /gateway/admin/kyc/pending`
- `PUT /gateway/admin/kyc/{kycId}/review`
- `GET /gateway/admin/users`
- `PUT /gateway/admin/users/{userId}/toggle`
- `GET /gateway/admin/dashboard`

## Current Project Status

Backend status:

- `AuthService` ✅
- `UserService` ✅
- `WalletService` ✅
- `RewardsService` ✅
- `NotificationService` ✅
- `AdminService` ✅
- `API Gateway` ✅
- `Shared.Events` ✅

Remaining major work:

- Angular UI integration
- End-to-end/integration test coverage
- Production hardening and deployment pipeline

## Folder Structure

```text
ZyntraPay/
├── AdminService/
├── ApiGateway/
├── AuthService/
├── NotificationService/
├── RewardsService/
├── Shared.Events/
├── UserService/
├── WalletService/
├── Database/
├── Diagrams/
└── README.md