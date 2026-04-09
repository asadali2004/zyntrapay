# Docker Setup Guide

## Purpose
This document explains how to run the ZyntraPay backend stack with Docker Compose.

## What Gets Started

The Docker stack includes:

- SQL Server
- RabbitMQ with management UI
- Mailpit for local email capture
- ApiGateway
- AuthService
- UserService
- WalletService
- RewardsService
- NotificationService
- AdminService

## Files Added for Docker

- [docker-compose.yml](e:\DotNet_Learning\ZyntraPay\Backend\docker-compose.yml)
- [\.dockerignore](e:\DotNet_Learning\ZyntraPay\Backend\.dockerignore)
- [\.env.example](e:\DotNet_Learning\ZyntraPay\Backend\.env.example)
- Dockerfile in each service and gateway project
- [ocelot.Docker.json](e:\DotNet_Learning\ZyntraPay\Backend\src\gateway\ApiGateway\ocelot.Docker.json)

## First-Time Run

From the `Backend` folder:

```powershell
docker compose up --build
```

If you want to run in detached mode:

```powershell
docker compose up --build -d
```

## Default Local Endpoints

- Gateway: `http://localhost:5000`
- AuthService: `http://localhost:5002`
- UserService: `http://localhost:5004`
- WalletService: `http://localhost:5006`
- RewardsService: `http://localhost:5008`
- NotificationService: `http://localhost:5010`
- AdminService: `http://localhost:5012`
- RabbitMQ UI: `http://localhost:15672`
- Mailpit UI: `http://localhost:8025`
- SQL Server: `localhost,1433`

## Docker Environment Notes

- Services talk to each other using container names, not `localhost`
- Gateway uses `ocelot.Docker.json` inside Docker
- All services listen on internal port `8080` inside containers
- SQL Server uses one instance with separate databases per service
- EF Core migrations still run automatically on startup

## Secrets and Overrides

Docker Compose includes safe local defaults for:

- SQL SA password
- JWT secret
- admin registration secret
- RabbitMQ credentials

If you want to override them, copy `.env.example` to `.env` and update values before running compose.

## Email Behavior in Docker

NotificationService is configured to use Mailpit locally:

- SMTP host: `mailpit`
- SMTP port: `1025`
- Mail UI: `http://localhost:8025`

This means local OTP and notification emails should be captured in Mailpit instead of requiring your real SMTP account.

## Stop the Stack

```powershell
docker compose down
```

To also remove the SQL volume:

```powershell
docker compose down -v
```

## Recommended Verification Order

1. Open `http://localhost:15672` and confirm RabbitMQ is up
2. Open `http://localhost:8025` and confirm Mailpit is up
3. Open `http://localhost:5000/health`
4. Test gateway auth endpoint through `/gateway/auth/...`
5. Trigger OTP or welcome email flow and verify message appears in Mailpit

## Notes

- Docker is the final local backend packaging step before Angular integration
- Frontend should call only the gateway, not service container URLs directly
