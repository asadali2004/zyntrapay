# ZyntraPay Project Command Reference

This file is the single command guide for this project.

Use it when you want to:

- build frontend
- run frontend
- build one backend service
- run one backend service
- run complete backend
- run tests
- use Docker

## 1. Folder Locations

Repository root:

```powershell
e:\DotNet_Learning\ZyntraPay
```

Frontend app:

```powershell
e:\DotNet_Learning\ZyntraPay\Frontend\zyntrapay-app
```

Backend solution:

```powershell
e:\DotNet_Learning\ZyntraPay\Backend
```

## 2. Frontend Commands

Move to frontend folder:

```powershell
cd Frontend\zyntrapay-app
```

Install frontend packages:

```powershell
npm install
```

Run frontend in development mode:

```powershell
ng serve
```

Frontend URL:

```text
http://localhost:4200
```

If `ng` is not recognized on your machine, use:

```powershell
npx ng serve
```

Build frontend:

```powershell
ng build
```

Build frontend for production:

```powershell
ng build --configuration production
```

Run frontend tests:

```powershell
ng test
```

## 3. Backend Common Commands

Move to backend folder:

```powershell
cd Backend
```

Restore all backend projects:

```powershell
dotnet restore .\ZyntraPay.slnx
```

Build complete backend:

```powershell
dotnet build .\ZyntraPay.slnx
```

Build complete backend in Release:

```powershell
dotnet build .\ZyntraPay.slnx --configuration Release
```

Run all backend tests:

```powershell
dotnet test .\ZyntraPay.slnx
```

Run all backend tests in Release:

```powershell
dotnet test .\ZyntraPay.slnx --configuration Release
```

## 4. Run One Backend Service

First start infrastructure services:

```powershell
cd Backend
docker compose up -d sqlserver rabbitmq mailpit
```

Then open a new terminal and run only the service you want.

### API Gateway

```powershell
cd Backend
dotnet run --project .\src\gateway\ApiGateway\ApiGateway.csproj
```

URL:

```text
http://localhost:5000
```

### AuthService

```powershell
cd Backend
dotnet run --project .\src\services\AuthService\AuthService.csproj
```

URL:

```text
http://localhost:5002
```

### UserService

```powershell
cd Backend
dotnet run --project .\src\services\UserService\UserService.csproj
```

URL:

```text
http://localhost:5004
```

### WalletService

```powershell
cd Backend
dotnet run --project .\src\services\WalletService\WalletService.csproj
```

URL:

```text
http://localhost:5006
```

### RewardsService

```powershell
cd Backend
dotnet run --project .\src\services\RewardsService\RewardsService.csproj
```

URL:

```text
http://localhost:5008
```

### NotificationService

```powershell
cd Backend
dotnet run --project .\src\services\NotificationService\NotificationService.csproj
```

URL:

```text
http://localhost:5010
```

### AdminService

```powershell
cd Backend
dotnet run --project .\src\services\AdminService\AdminService.csproj
```

URL:

```text
http://localhost:5012
```

## 5. Build One Backend Service

AuthService:

```powershell
dotnet build .\src\services\AuthService\AuthService.csproj
```

UserService:

```powershell
dotnet build .\src\services\UserService\UserService.csproj
```

WalletService:

```powershell
dotnet build .\src\services\WalletService\WalletService.csproj
```

RewardsService:

```powershell
dotnet build .\src\services\RewardsService\RewardsService.csproj
```

NotificationService:

```powershell
dotnet build .\src\services\NotificationService\NotificationService.csproj
```

AdminService:

```powershell
dotnet build .\src\services\AdminService\AdminService.csproj
```

ApiGateway:

```powershell
dotnet build .\src\gateway\ApiGateway\ApiGateway.csproj
```

## 6. Run Complete Backend Locally

If you want the full backend without running all services in Docker, use this order.

### Step 1. Start infrastructure

```powershell
cd Backend
docker compose up -d sqlserver rabbitmq mailpit
```

### Step 2. Open separate terminals and run all services

Terminal 1:

```powershell
cd Backend
dotnet run --project .\src\services\AuthService\AuthService.csproj
```

Terminal 2:

```powershell
cd Backend
dotnet run --project .\src\services\UserService\UserService.csproj
```

Terminal 3:

```powershell
cd Backend
dotnet run --project .\src\services\WalletService\WalletService.csproj
```

Terminal 4:

```powershell
cd Backend
dotnet run --project .\src\services\RewardsService\RewardsService.csproj
```

Terminal 5:

```powershell
cd Backend
dotnet run --project .\src\services\NotificationService\NotificationService.csproj
```

Terminal 6:

```powershell
cd Backend
dotnet run --project .\src\services\AdminService\AdminService.csproj
```

Terminal 7:

```powershell
cd Backend
dotnet run --project .\src\gateway\ApiGateway\ApiGateway.csproj
```

Main backend entry point:

```text
http://localhost:5000
```

Support tools:

- RabbitMQ UI: `http://localhost:15672`
- Mailpit UI: `http://localhost:8025`
- SQL Server: `localhost,1433`

## 7. Run Complete Backend With Docker

Move to backend folder:

```powershell
cd Backend
```

Build and run full backend stack:

```powershell
docker compose up --build
```

Build and run full backend stack in detached mode:

```powershell
docker compose up --build -d
```

Stop containers:

```powershell
docker compose down
```

Stop containers and remove volumes:

```powershell
docker compose down -v
```

See running containers:

```powershell
docker compose ps
```

Check backend health through gateway:

```powershell
Invoke-WebRequest http://localhost:5000/health -UseBasicParsing
```

Docker URLs:

- Gateway: `http://localhost:5000`
- AuthService: `http://localhost:5002`
- UserService: `http://localhost:5004`
- WalletService: `http://localhost:5006`
- RewardsService: `http://localhost:5008`
- NotificationService: `http://localhost:5010`
- AdminService: `http://localhost:5012`
- RabbitMQ UI: `http://localhost:15672`
- Mailpit UI: `http://localhost:8025`

## 8. Test Commands

Run one backend test project:

```powershell
dotnet test .\test\AuthService.Tests\AuthService.Tests.csproj
dotnet test .\test\UserService.Tests\UserService.Tests.csproj
dotnet test .\test\WalletService.Tests\WalletService.Tests.csproj
dotnet test .\test\RewardsService.Tests\RewardsService.Tests.csproj
dotnet test .\test\NotificationService.Tests\NotificationService.Tests.csproj
dotnet test .\test\AdminService.Tests\AdminService.Tests.csproj
dotnet test .\test\ZyntraPay.IntegrationTests\ZyntraPay.IntegrationTests.csproj
```

## 9. Recommended Daily Flows

### Frontend only

```powershell
cd Frontend\zyntrapay-app
npm install
ng serve
```

### One backend service only

```powershell
cd Backend
docker compose up -d sqlserver rabbitmq mailpit
dotnet run --project .\src\services\AuthService\AuthService.csproj
```

### Complete backend in Docker

```powershell
cd Backend
docker compose up --build -d
```

### Complete project for normal development

Terminal 1:

```powershell
cd Backend
docker compose up -d sqlserver rabbitmq mailpit
```

Terminal 2 to Terminal 8:

Run backend services and gateway from section 6.

Terminal 9:

```powershell
cd Frontend\zyntrapay-app
ng serve
```

## 10. Quick Notes

- Frontend command is `ng serve`, not `npm start`
- Frontend should call the gateway, not backend service URLs directly
- For full local backend, start infrastructure before running services
- For simplest full-backend run, use Docker Compose
