# ZyntraPay — Next Steps Implementation Plan

## Objective
This document defines what must be implemented next to complete the case study as per expected backend + Angular microservices standards.

---

## 0) Current Backend Snapshot (Updated)

### Completed / In Place
- Core microservices implemented: `AuthService`, `UserService`, `WalletService`, `RewardsService`, `NotificationService`, `AdminService`, `ApiGateway`.
- Unit test projects implemented for: `AuthService`, `UserService`, `WalletService`, `RewardsService`.
- Integration tests implemented for key auth/wallet flows.
- `Forgot Password`, `Reset Password`, and `Refresh Token` flows implemented in `AuthService`.
- Polly resilience implemented in `AdminService` downstream HTTP clients.
- Rewards catalog caching implemented.
- Global exception middleware and logging implemented across services.

### Pending / Partial
- `AdminService` unit test project needs final wiring into active solution + execution evidence.
- Coverage is currently below target; service-layer coverage needs to be improved.
- Angular frontend implementation pending.

---

## 1) Highest Priority (Must Complete)

## 1.1 Testing (Backend)

### Unit Testing (NUnit)
Create test projects and cover core business logic:

- `AuthService`:
  - register success/failure (duplicate email/phone)
  - login success/failure
  - admin register secret validation
  - forgot/reset password rules
  - refresh-token success/failure
- `UserService`:
  - profile create/get
  - KYC submit/get
  - KYC review rules
- `WalletService`:
  - create wallet
  - top-up flow
  - transfer flow (insufficient balance, self-transfer, inactive wallet)
- `RewardsService`:
  - points awarding logic
  - redemption success/failure
- `AdminService`:
  - review KYC orchestration
  - toggle user status orchestration

### Integration Testing
Add API-level tests for key flows via in-memory test host / test DB:

- Auth -> login token generation
- Wallet top-up -> event publish path (at least mocked/asserted)
- Admin -> downstream client flow

### Coverage
Generate coverage report and target a practical baseline (example: `>=70%` service layer).

---

## 1.2 Frontend (Angular) + Integration

Implement Angular app integration with gateway endpoints:

### Mandatory Modules
- Auth module (register/login)
- User module (profile + KYC)
- Wallet module (create, balance, top-up, transfer, transactions)
- Rewards module (summary, catalog, redeem, history)
- Notifications module (list + mark read)
- Admin module (users, KYC review, dashboard)

### Angular Best Practices to Implement
- Feature-based module structure
- Reactive forms + validation
- HTTP interceptor for JWT
- Route guards (`AuthGuard`, `AdminGuard`)
- Shared reusable components
- Service-only API calls
- Toast notifications for success/error
- Pagination where list data grows

### Integration Checklist
- Gateway base URL from environment config
- Token persistence and auto-attach in requests
- Role-based route visibility
- Error handling UX across all API failures

---

## 1.3 Static Code Analysis

- Enable SonarLint in Visual Studio.
- Resolve major/high issues in all services.
- Optional: add SonarQube scan in CI.

---

## 2) Important Improvements (Should Complete)

## 2.1 Resilience for Inter-Service Calls
Apply Polly policies in `AdminService` HTTP clients:

- Retry policy
- Circuit breaker
- Timeout policy

## 2.2 Caching
Add caching for read-heavy endpoints:

- Rewards catalog (`MemoryCache` initially)
- Potential dashboard summary caching

## 2.3 Observability and Logging
- Standardize structured logging format
- Add correlation id propagation (gateway -> downstream)

## 2.4 Security Hardening
- Add stricter password policy checks
- Add claims-based policy examples
- Secure secrets for non-dev environments
- Avoid returning raw exception details in production responses
- Add OTP/reset attempt throttling and lockout strategy

---

## 3) Deployment Readiness (Good to Have but Valuable)

## 3.1 Dockerization
- Add `Dockerfile` for each service + gateway
- Add `docker-compose.yml` for all services + SQL Server + RabbitMQ

## 3.2 CI/CD
- Add GitHub Actions pipeline:
  - restore/build
  - test + coverage
  - static analysis
  - publish artifacts

---

## 4) Documentation/Presentation Finalization

- Keep service docs updated with final API list.
- Add architecture diagram export references in docs.
- Prepare viva-ready explanation for:
  - why separate DB per service
  - event-driven flow and eventual consistency
  - JWT role security flow
  - layered architecture decisions

---

## 5) Suggested Execution Order (Backend-First Updated)

1. Finalize `AdminService` unit test wiring and execution proof
2. Increase service-layer coverage (focus auth reset/refresh + admin orchestration)
3. Add integration tests for admin downstream flow
4. Apply security hardening quick wins
5. Start Angular modules and gateway integration
6. Add Docker + CI/CD
7. Final documentation and viva preparation

---

## 6) Definition of Done (Case Study)

Mark case study as complete when all are true:

- Backend microservices stable and tested
- Angular frontend integrated with all required user/admin flows
- Unit + integration test evidence available
- Coverage report generated
- Static analysis issues addressed
- Documentation complete and consistent
- Project demo and viva preparation checklist completed
