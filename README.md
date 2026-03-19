# Digital Wallet & Rewards System

A microservices-based Digital Wallet and Loyalty/Rewards platform built as a final evaluation project.

## Tech Stack

- **Backend:** .NET 8 Web API (Microservices)
- **Frontend:** Angular
- **Database:** SQL Server (EF Core DB First)
- **Auth:** JWT Bearer Tokens
- **Messaging:** RabbitMQ
- **Gateway:** Ocelot API Gateway

## Services & Ports

| Service             | Port |
|---------------------|------|
| API Gateway         | 5001 |
| AuthService         | 5003 |
| UserService         | 5005 |
| WalletService       | 5007 |
| RewardsService      | 5009 |
| NotificationService | 5011 |
| AdminService        | 5013 |
| Angular Frontend    | 4200 |

## Architecture
```
Angular (4200)
     │
     ▼
API Gateway (5001)
     │
     ├── /gateway/auth/*         → AuthService        (5003)
     ├── /gateway/user/*         → UserService        (5005)
     ├── /gateway/wallet/*       → WalletService      (5007)
     ├── /gateway/rewards/*      → RewardsService     (5009)
     ├── /gateway/notification/* → NotificationService(5011)
     └── /gateway/admin/*        → AdminService       (5013)

Event Flow (RabbitMQ):
  WalletService → WalletTopUpCompleted   → RewardsService, NotificationService
  RewardsService → PointsAwarded        → NotificationService
  AdminService   → KycStatusChanged     → NotificationService

Admin → UserService (HTTP) for KYC approval
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (SSMS)
- RabbitMQ
- Node.js (for Angular)

### Database Setup
Run SQL scripts in order from the `Database/` folder:

| Script               | Database        |
|----------------------|-----------------|
| 01_AuthDB.sql        | AuthDB          |
| 02_UserDB.sql        | UserDB          |
| 03_WalletDB.sql      | WalletDB        |
| 04_RewardsDB.sql     | RewardsDB       |
| 05_NotificationDB.sql| NotificationDB  |
| 06_AdminDB.sql       | AdminDB         |

### Run the Project
1. Run all SQL scripts in SSMS in order
2. Update connection strings in each service's `appsettings.json`
3. In Visual Studio → right-click Solution → Properties → Multiple Startup Projects → set all services to Start
4. Press F5 — all services start together

### Admin Setup
To register an admin user, use the secret key defined in `AuthService/appsettings.json`:
```
POST /gateway/auth/register-admin
{
  "email": "admin@digitalwallet.com",
  "phoneNumber": "9000000001",
  "password": "Admin@123",
  "adminSecretKey": "AdminSecret@2024"
}
```

### API Documentation
Each service exposes Swagger UI at `/swagger`

| Service             | Swagger URL                       |
|---------------------|-----------------------------------|
| AuthService         | https://localhost:5003/swagger    |
| UserService         | https://localhost:5005/swagger    |
| WalletService       | https://localhost:5007/swagger    |
| RewardsService      | https://localhost:5009/swagger    |
| NotificationService | https://localhost:5011/swagger    |
| AdminService        | https://localhost:5013/swagger    |

## API Reference

### AuthService (5003)
| Method | Endpoint                        | Access        | Description              |
|--------|---------------------------------|---------------|--------------------------|
| POST   | /gateway/auth/register          | Public        | Register new user        |
| POST   | /gateway/auth/register-admin    | Secret Key    | Register admin user      |
| POST   | /gateway/auth/login             | Public        | Login, returns JWT token |

### UserService (5005)
| Method | Endpoint                        | Access        | Description              |
|--------|---------------------------------|---------------|--------------------------|
| POST   | /gateway/user/profile           | User JWT      | Create user profile      |
| GET    | /gateway/user/profile           | User JWT      | Get own profile          |
| POST   | /gateway/user/kyc               | User JWT      | Submit KYC documents     |
| GET    | /gateway/user/kyc               | User JWT      | Get KYC status           |

## Key Design Decisions

**Why separate DB per service?**
Each service owns its data. If WalletDB goes down, UserService still works. Loose coupling at the data layer.

**Why JWT over sessions?**
Stateless — every service validates the token independently without calling AuthService. Scales naturally.

**Why AuthUserId instead of FK across DBs?**
Cross-database foreign keys are not possible in microservices. The relationship is enforced at the application level.

**Why RabbitMQ instead of direct HTTP between services?**
Async and decoupled. WalletService does not wait for RewardsService to finish. More resilient to failures.

**Why Repository Pattern?**
Separates data access from business logic. Controllers stay clean, services stay testable.

**Why manual DTOs over AutoMapper?**
Every mapping is explicit and traceable. No magic. Easy to explain and debug.

## Diagrams

All architecture and service diagrams are in the `Diagrams/` folder.

| Diagram                              | Description                        |
|--------------------------------------|------------------------------------|
| 00_Overall_Architecture_Diagram      | Full system overview               |
| 01_AuthService_Design                | AuthService layers and AuthDB      |
| 02_UserService_Design                | UserService layers and UserDB      |

## Project Status

| Day   | Status     | What Was Built                                              |
|-------|------------|-------------------------------------------------------------|
| Day 1 | ✅ Done    | AuthService, Ocelot API Gateway, JWT, Swagger, port setup  |
| Day 2 | 🔄 In Progress | UserService (profile + KYC), WalletService              |
| Day 3 | ⏳ Pending | RewardsService, RabbitMQ event-driven communication        |
| Day 4 | ⏳ Pending | AdminService (KYC approval, dashboard), Angular UI         |
| Day 5 | ⏳ Pending | Integration, testing, polish, viva prep                    |

## Folder Structure
```
digital-wallet-dotnet/
├── AdminService/
├── ApiGateway/
├── AuthService/
├── NotificationService/
├── RewardsService/
├── Shared.Events/
├── UserService/
├── WalletService/
├── Database/          ← SQL scripts numbered in execution order
├── Diagrams/          ← Excalidraw design files + PNG exports
├── .gitignore
├── README.md
└── DigitalWallet.sln
```