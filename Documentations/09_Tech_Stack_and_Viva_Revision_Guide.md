# ZyntraPay — Tech Stack and Viva Revision Guide

## Purpose
This document lists all technologies and concepts already used (or expected to be used next) in this case study, and what to revise before viva.

---

## 1) Current + Target Tech Stack

## Backend
- `C#` (`.NET 8`)
- `ASP.NET Core Web API`
- `Entity Framework Core`
- `SQL Server`
- `JWT Bearer Authentication`
- `Role-based Authorization`
- `Ocelot API Gateway`
- `RabbitMQ` (event-driven messaging)
- `Swagger / Swashbuckle`
- `ILogger` logging
- Global exception middleware

## Frontend
- `Angular` (your selected frontend stack)
- Angular routing, guards, reactive forms
- HttpClient with interceptor

## Engineering/Quality (to add/strengthen)
- `NUnit` testing
- Integration tests
- Code coverage report
- SonarLint / SonarQube
- Docker + docker-compose
- CI/CD (GitHub Actions or Azure DevOps)

---

## 2) C# Topics to Revise

- OOP pillars: encapsulation, inheritance, polymorphism, abstraction
- Interfaces and dependency inversion
- Collections and generics
- Async/await and task-based programming
- LINQ fundamentals and query composition
- Lambda expressions and delegates
- Exception handling best practices
- Nullable reference types
- Record/class differences (as relevant)
- DateTime handling (`UtcNow` vs local time)

Viva focus:
- Why async in APIs?
- Why interfaces for services/repositories?
- How LINQ maps to SQL in EF Core?

---

## 3) ASP.NET Core / Web API Topics

- Middleware pipeline and ordering
- Dependency Injection lifetimes (`Singleton`, `Scoped`, `Transient`)
- Model binding and validation attributes
- Controller/action conventions
- Global exception middleware
- `IConfiguration` and environment-based settings
- Logging with `ILogger`
- CORS handling

Viva focus:
- Why middleware for exceptions?
- Why `Scoped` for repository/service in EF context?
- Pipeline order effects (`UseAuthentication` before `UseAuthorization`)

---

## 4) Authentication and Security Topics

- JWT token structure and claims
- Token validation params (issuer, audience, key, lifetime)
- Role-based authorization with `[Authorize(Roles="...")]`
- Claims-based authorization concept
- Secure secret management for production

Viva focus:
- Where token is generated and where validated?
- Difference between authentication and authorization?
- Why role checks at controller/action?

---

## 5) EF Core + SQL Server Topics

- DbContext and DbSet usage
- Migrations lifecycle
- Fluent API and constraints/indexes
- Relationship mapping and delete behaviors
- Tracking vs no-tracking queries
- Transaction consistency basics
- Indexing and query performance basics

Viva focus:
- Why one DB per service?
- Why use EF Core + repository pattern?
- How migrations are auto-applied in your services?

---

## 6) Microservices Architecture Topics

- Bounded context per service
- Independent deployment and scaling
- Synchronous vs asynchronous communication
- Eventual consistency
- API Gateway role
- Service ownership of data

Viva focus:
- Why microservices over monolith for this case?
- Trade-offs (complexity, distributed debugging, consistency)
- How service boundaries were defined?

---

## 7) RabbitMQ/Event-Driven Topics

- Producer/consumer pattern
- Queue durability and persistent messages
- Manual ACK/NACK behavior
- Retry/requeue strategies
- Idempotency concept (important in event processing)
- Event contract sharing via `Shared.Events`

Viva focus:
- Why event-driven for rewards/notifications?
- What happens if consumer is down?
- How do you avoid message loss?

---

## 8) API Gateway (Ocelot) Topics

- Upstream vs downstream route mapping
- Route templates
- Centralized entry point advantages
- CORS at gateway level

Viva focus:
- Why gateway if services are directly reachable?
- How route rewriting works in your setup?

---

## 9) Angular Topics to Revise (Must)

- Angular project architecture and modules
- Reactive forms + validators
- Routing and lazy loading
- Route guards and role-based navigation
- HttpClient services and interceptors
- State handling (service state / RxJS patterns)
- Error handling and toast notifications
- Reusable components and shared modules

Viva focus:
- How token is stored and attached?
- How admin routes are protected?
- Why reactive forms over template forms?

---

## 10) Testing Topics (Must Implement + Revise)

- NUnit test structure (`Arrange-Act-Assert`)
- Mocking dependencies (e.g., Moq)
- Unit test vs integration test differences
- Testing service layer business rules
- Coverage metrics and interpretation

Viva focus:
- Which business rules are unit tested?
- How do integration tests differ from unit tests?
- Current coverage and improvement plan?

---

## 11) Quality, DevOps, and Production-Readiness Topics

- SonarLint issues and fixes
- Docker fundamentals (`Dockerfile`, multi-service compose)
- CI/CD basics for .NET projects
- Resilience with Polly (retry/circuit breaker)
- Caching basics (`MemoryCache`, optional Redis)

Viva focus:
- How would you deploy this to production?
- What resilience patterns are implemented/planned?
- Where caching fits in your design?

---

## 12) Final Viva Prep Checklist

Before viva, make sure you can confidently explain:

- End-to-end user flow across services
- Auth token lifecycle
- Wallet -> Rewards/Notifications event flow
- Admin orchestration flow with token forwarding
- Database ownership and microservice boundaries
- Testing strategy and quality measures
- Planned improvements (Polly, Docker, CI/CD, caching)

Also keep ready:
- API demo sequence
- Architecture diagram
- One-page summary of key design decisions
