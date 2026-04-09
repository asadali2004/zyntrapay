# Backend Patterns and Principles (Current Status)

This document summarizes which backend principles and patterns are currently implemented in ZyntraPay, and which are not.

## SOLID Principles

**Partially applied (good baseline):**
- **Single Responsibility (SRP):** Controllers, services, repositories, and middleware are separated by responsibility.
- **Open/Closed (OCP):** Services use interfaces; most changes can be extended by adding new implementations.
- **Liskov Substitution (LSP):** Not explicitly tested, but interface usage is standard and safe.
- **Interface Segregation (ISP):** Services are split into focused interfaces (`IAuthService`, `IUserService`, etc.).
- **Dependency Inversion (DIP):** Dependencies are injected via interfaces throughout the services.

**Summary:** SOLID is followed at a practical level for a case study. It is not fully “textbook enforced,” but the structure supports it.

## SAGA Pattern

**Not implemented.**
- We use RabbitMQ for event-driven flows, but there is no Saga orchestration or compensating transactions across services.

If asked in viva: say we are using **event-driven communication**, and Saga could be added later for distributed transactions.

## CORS

**Implemented at gateway level only.**
- `ApiGateway` has a named CORS policy (`AllowAngular`).
- Individual microservices do not configure CORS directly.

This is correct because all frontend traffic should go through the gateway.

## CQRS

**Not implemented.**
- Reads and writes happen in the same services and same models.
- No command/query separation or separate read models.

If asked: CQRS was not required for the scope, but could be adopted later for high-scale read workloads.

## Patterns Currently Used

1. **Layered Architecture**
   - Controller → Service → Repository → DbContext

2. **Repository Pattern**
   - Data access isolated from business logic

3. **DTO Pattern**
   - DTOs used for API requests/responses (no EF entities exposed)

4. **Dependency Injection (DI)**
   - Services and repositories injected via interfaces

5. **Global Exception Middleware**
   - Consistent error handling in each service

6. **JWT Authentication + Role-Based Authorization**
   - JWT bearer auth in services, `Authorize(Roles = "Admin")` where needed

7. **API Gateway Pattern**
   - Ocelot gateway routes all frontend traffic

8. **Event-Driven Architecture (RabbitMQ)**
   - Pub/Sub for OTP, wallet, rewards, KYC, notifications

9. **Retry/Circuit Breaker/Timeout (Polly)**
   - Implemented in AdminService for downstream HTTP calls

10. **Cache-Aside Pattern**
   - `MemoryCache` used for wallet balance, rewards catalog, and OTPs

11. **Test Patterns**
   - NUnit unit tests
   - Integration tests via `WebApplicationFactory`

## Patterns Not Implemented Yet (Optional/Advanced)

- Saga orchestration
- Outbox pattern for guaranteed event delivery
- CQRS
- Distributed caching (Redis)
- Full CI/CD pipeline
- Docker compose for full environment

## Summary for Viva

We are using practical patterns suitable for the case study:
- Layered architecture with DTOs and repositories
- JWT security and role-based access
- RabbitMQ event-driven flows
- Polly resilience where HTTP calls exist
- Caching for performance

Advanced patterns (Saga, CQRS, Outbox) are acknowledged but not required for this scope.
