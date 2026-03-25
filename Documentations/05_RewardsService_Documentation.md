# RewardsService Documentation

## Responsibility

`RewardsService` is responsible for loyalty points, tier tracking, reward catalog, and redemption lifecycle.

## What It Does

- Consumes wallet top-up events and awards points.
- Creates reward account on first points event.
- Calculates and updates user tier.
- Serves reward catalog and summary.
- Processes redemptions and maintains redemption history.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| Framework | `ASP.NET Core Web API` |
| Data Access | `EF Core` |
| Database | `SQL Server` |
| Auth | `JWT Bearer` |
| Messaging | `RabbitMQ.Client` (consumer) |
| API Docs | `Swagger` |

## Runtime and Data

| Item | Value |
|---|---|
| Port | `5009` |
| Base Route | `/api/rewards` |
| Database | `RewardsDB` |
| Tables | `RewardAccounts`, `RewardCatalog`, `Redemptions` |

## APIs and Responsible Methods

| Method | API | Auth | Service Method | Purpose |
|---|---|---|---|---|
| `GET` | `/api/rewards/summary` | User | `GetSummaryAsync` | Get points and tier |
| `GET` | `/api/rewards/catalog` | User | `GetCatalogAsync` | Get active catalog |
| `POST` | `/api/rewards/redeem` | User | `RedeemAsync` | Redeem reward item |
| `GET` | `/api/rewards/history` | User | `GetHistoryAsync` | Get redemption history |
| Event | `WalletTopUpCompletedEvent` | Internal | `AwardPointsAsync` | Award points from top-up |

## Complete Workflow

1. Wallet top-up event is received by `WalletTopUpConsumer`.
2. Consumer calls `AwardPointsAsync`.
3. Points are calculated (`1 point per Rs.100`).
4. Account is created/updated and tier recalculated.
5. User can query summary/catalog/history via APIs.
6. User redemption deducts points, updates stock, and stores redemption record.

## Integrations

| Integration Type | Source | Target | Purpose |
|---|---|---|---|
| HTTP | Client/Gateway | `RewardsService` | Rewards APIs |
| RabbitMQ | `WalletService` | `RewardsService` | Top-up event consumption |

## Key Methods and Tasks

| Class | Method/Task |
|---|---|
| `RewardsController` | Expose rewards APIs |
| `RewardsServiceImpl` | Business logic for points and redemption |
| `WalletTopUpConsumer` | Event listener for top-up messages |
| `TierHelper` | Points and tier calculation |
| `RewardsRepository` | DB operations for rewards domain |
| `RewardsDbContext` | Entity mapping and seed data |

## Key Files

- `RewardsService/Controllers/RewardsController.cs`
- `RewardsService/Services/RewardsServiceImpl.cs`
- `RewardsService/Consumers/WalletTopUpConsumer.cs`
- `RewardsService/Helpers/TierHelper.cs`
- `RewardsService/Repositories/RewardsRepository.cs`
- `RewardsService/Data/RewardsDbContext.cs`
- `RewardsService/DTOs/RewardsDTOs.cs`
