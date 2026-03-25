# ApiGateway Documentation

## Responsibility

`ApiGateway` is the single external entry point for API requests. It is responsible for route forwarding and CORS policy, not domain business logic.

## What It Does

- Receives client requests on gateway routes.
- Maps upstream URLs to downstream service URLs.
- Applies development CORS policy for Angular.
- Forwards request/response between client and service.

## Technologies Used

| Area | Technology |
|---|---|
| Runtime | `.NET 8` |
| Framework | `ASP.NET Core` |
| Gateway Routing | `Ocelot` |
| CORS | ASP.NET Core CORS middleware |

## Runtime and Data

| Item | Value |
|---|---|
| Port | `5001` |
| Base URL | `https://localhost:5001` |
| Database | N/A |

## APIs and Responsible Routing

| Upstream Route | Downstream Route | Target |
|---|---|---|
| `/gateway/auth/{everything}` | `/api/auth/{everything}` | `https://localhost:5003` |
| `/gateway/user/{everything}` | `/api/user/{everything}` | `https://localhost:5005` |
| `/gateway/wallet/{everything}` | `/api/wallet/{everything}` | `https://localhost:5007` |
| `/gateway/rewards/{everything}` | `/api/rewards/{everything}` | `https://localhost:5009` |
| `/gateway/notification/{everything}` | `/api/notification/{everything}` | `https://localhost:5011` |
| `/gateway/admin/{everything}` | `/api/admin/{everything}` | `https://localhost:5013` |

## Complete Workflow

1. Client sends request to gateway (example: `/gateway/wallet/balance`).
2. Ocelot matches route from `ocelot.json`.
3. Gateway rewrites path to downstream pattern.
4. Request is forwarded to the target service.
5. Response is returned back to client.

## Integrations

| Integration Type | Source | Target | Purpose |
|---|---|---|---|
| HTTP | Client | `ApiGateway` | Entry point |
| HTTP | `ApiGateway` | All microservices | Route forwarding |

## Key Methods and Tasks

| File | Key Task |
|---|---|
| `ApiGateway/Program.cs` | Load config, register Ocelot, configure CORS, run middleware |
| `ApiGateway/ocelot.json` | Define route mappings |

## Key Files

- `ApiGateway/Program.cs`
- `ApiGateway/ocelot.json`
- `ApiGateway/appsettings.json`
