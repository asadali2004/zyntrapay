# WalletService Documentation

## Responsibility

`WalletService` is responsible for wallet lifecycle, balance management, ledger entries, transfers, and wallet-related event publishing.

## What It Does

- Creates wallet for authenticated users.
- Maintains current balance snapshot.
- Writes ledger entries for top-up and transfer.
- Returns transaction history/details.
- Publishes wallet events to RabbitMQ after successful DB updates.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| Framework | `ASP.NET Core Web API` |
| Data Access | `EF Core` |
| Database | `SQL Server` |
| Auth | `JWT Bearer` |
| Messaging | `RabbitMQ.Client` |
| API Docs | `Swagger` |

## Runtime and Data

| Item | Value |
|---|---|
| Port | `5007` |
| Base Route | `/api/wallet` |
| Database | `WalletDB` |
| Tables | `Wallets`, `LedgerEntries` |

## APIs and Responsible Methods

| Method | API | Auth | Service Method | Purpose |
|---|---|---|---|---|
| `POST` | `/api/wallet/create` | User | `CreateWalletAsync` | Create wallet |
| `GET` | `/api/wallet/balance` | User | `GetBalanceAsync` | Get wallet balance |
| `POST` | `/api/wallet/topup` | User | `TopUpAsync` | Top-up wallet and publish event |
| `POST` | `/api/wallet/transfer` | User | `TransferAsync` | Transfer money and publish event |
| `GET` | `/api/wallet/transactions` | User | `GetTransactionsAsync` | Get transaction history |
| `GET` | `/api/wallet/transactions/{id}` | User | `GetTransactionByIdAsync` | Get one transaction |

## Complete Workflow

1. User calls wallet API with JWT.
2. Service resolves user id from token.
3. Service validates wallet status and business conditions.
4. For monetary operations, ledger entries are created.
5. Wallet balance is updated and saved.
6. For top-up/transfer, event is published to RabbitMQ.
7. Result response is returned.

## Integrations

| Integration Type | Source | Target | Purpose |
|---|---|---|---|
| HTTP | Client/Gateway | `WalletService` | Wallet APIs |
| RabbitMQ | `WalletService` | `RewardsService` | Top-up event for points |
| RabbitMQ | `WalletService` | `NotificationService` | Top-up/transfer notifications |

## Key Methods and Tasks

| Class | Method/Task |
|---|---|
| `WalletController` | Expose wallet APIs and extract auth user id |
| `WalletServiceImpl` | Core wallet business logic |
| `RabbitMqPublisher` | Publish integration events |
| `WalletRepository` | Wallet and ledger persistence |
| `WalletDbContext` | DB entity mapping |

## Key Files

- `WalletService/Controllers/WalletController.cs`
- `WalletService/Services/WalletServiceImpl.cs`
- `WalletService/Services/RabbitMqPublisher.cs`
- `WalletService/Repositories/WalletRepository.cs`
- `WalletService/Data/WalletDbContext.cs`
- `WalletService/DTOs/WalletDTOs.cs`
- `WalletService/Models/Wallet.cs`
- `WalletService/Models/LedgerEntry.cs`
