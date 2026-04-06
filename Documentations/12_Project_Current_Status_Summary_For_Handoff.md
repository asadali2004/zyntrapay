# ZyntraPay — Current Project Status Summary (Handoff Prompt Reference)

Use this document as the latest ground truth for continuing development in another chat.

---

## 1) Project Context

- Project: `ZyntraPay`
- Stack: `.NET 8` microservices backend
- API Gateway: `Ocelot`
- DB: `SQL Server` with `EF Core`
- Messaging: `RabbitMQ`
- Auth: `JWT Bearer` + role-based auth
- Frontend: `Angular` planned (not started yet)

Repository/Workspace root:
- `E:\DotNet_Learning\ZyntraPay`

---

## 2) Solution Structure (current)

Main service projects:
- `AuthService`
- `UserService`
- `WalletService`
- `RewardsService`
- `NotificationService`
- `AdminService`
- `ApiGateway`
- `Shared.Events`

Test projects:
- `AuthService.Tests`
- `UserService.Tests`
- `WalletService.Tests`
- `RewardsService.Tests`
- `ZyntraPay.IntegrationTests`
- `AdminService.Tests` 

---

## 3) What is Implemented (Backend)

### AuthService
Implemented flows:
- register (OTP-verified email flow)
- login
- admin register (secret key)
- send OTP / verify OTP
- forgot password
- reset password
- refresh token
- update phone
- get users (admin)
- toggle user status (admin)
- get email by auth user id

Notes:
- OTP and reset OTP use `IMemoryCache`.
- refresh token flow is implemented in service/controller.

### UserService
- profile create/get
- KYC submit/get status
- admin KYC review rules

### WalletService
- create wallet
- get balance
- top-up
- transfer
- transaction history

### RewardsService
- reward summary
- catalog with caching (`IMemoryCache`)
- redeem flow
- redemption history
- points awarding via event consumer (top-up event)

### NotificationService
- consumers for OTP/welcome/top-up/transfer/points/KYC events
- notification APIs (list/mark read)

### AdminService
- pending KYC list
- review KYC orchestration
- get users orchestration
- toggle user status orchestration
- dashboard summary orchestration
- resilience with `Polly` (retry/circuit breaker/timeout) on downstream HTTP clients

### Cross-cutting
- global exception middleware in services
- structured logging via `ILogger`
- swagger enabled
- db migration on startup

---

## 4) Event-Driven Flows Implemented

Using `Shared.Events` contracts.

Examples:
- wallet top-up -> rewards points + notification
- wallet transfer -> notification
- auth send OTP -> notification email consumer
- auth register success -> welcome email consumer
- admin KYC review -> KYC status change event

---

## 5) Testing Status (latest known)

### Unit Tests
- `AuthService.Tests`: expanded and passing (includes forgot/reset/refresh token tests)
- `UserService.Tests`: passing
- `WalletService.Tests`: passing
- `RewardsService.Tests`: passing
- `AdminService.Tests`: added and passing via direct project run (`dotnet test .\\AdminService.Tests\\AdminService.Tests.csproj`)

### Integration Tests
- `ZyntraPay.IntegrationTests` includes:
  - `AuthIntegrationTests`
  - `WalletIntegrationTests`
  - `AdminIntegrationTests` (newly added)
- latest run for integration project passed.

### Coverage
- coverage report has been generated earlier (`CoverageReport/index.html`).
- previously observed overall line coverage was around ~30% (before latest additions).
- more tests were added after that; coverage should be regenerated for updated numbers.

---

## 6) Recent Copilot Changes (important)

1. Added `AdminService.Tests` project and core admin unit tests (simple style, matching existing test pattern).
2. Expanded `AuthService.Tests` with tests for:
   - forgot password
   - reset password (expired/wrong/valid)
   - refresh token (invalid/valid/inactive)
3. Added Admin integration support:
   - updated `AdminService/Program.cs` to be integration-test friendly (`Migrate` vs `EnsureCreated`) and added `partial Program`.
   - updated `ZyntraPay.IntegrationTests.csproj` to reference `AdminService`.
   - added `AdminWebApplicationFactory` with in-memory admin db + fake downstream clients.
   - added `AdminIntegrationTests` for pending KYC, review KYC, dashboard.
4. Updated implementation plan document:
   - `Documentations/08_Next_Steps_Implementation_Plan.md`
   - username requirement was discussed then explicitly removed from immediate next steps (deferred).

---

## 7) Decisions/Scope Clarifications

- A new business requirement (`unique username`) was received from higher team.
- It was estimated as moderate impact.
- Current decision: **defer username implementation for now**.
- Immediate focus shifted to:
  1) admin test integration and evidence
  2) coverage improvement
  3) admin downstream integration tests

---

## 8) Known Gaps / Next Priorities

1. Regenerate and improve coverage toward service-layer target (`>=70%` aspirational from plan).
2. Keep improving negative-path tests in service and integration suites.
3. Security hardening quick wins:
   - avoid exposing raw exception details in production responses
   - improve OTP/reset abuse protection (attempt throttling/lockout)
   - strengthen password policy
4. Frontend Angular modules and gateway integration are still pending.
5. Docker/CI/CD pending (intentionally deferred).

---

## 9) Suggested Prompt to Continue in Another Chat

```text
You are continuing work on my .NET 8 microservices project "ZyntraPay".
Please treat the attached "Documentations/12_Project_Current_Status_Summary_For_Handoff.md" as the latest baseline and source of truth.
Important constraints:
- Keep code simple and fresher-friendly.
- Follow existing project coding style and test style.
- Do not over-engineer.
Current focus:
1) Improve backend test coverage (service layer first)
2) Add any missing admin/auth negative-path tests
3) Apply small security hardening improvements without major architecture changes
Do not start username implementation yet (deferred).
```

---

## 10) Style Constraints (must keep)

From repo instructions (`.github/copilot-instructions.md`):
- Keep changes in existing project coding style.
- Keep consistency with current style while making changes.
- Keep code simple; avoid over-engineering.
