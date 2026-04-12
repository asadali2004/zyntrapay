# ZyntraPay

ZyntraPay is organized with the backend source under [Backend](./Backend) and the Angular frontend under [Frontend](./Frontend).

For day-to-day build and run commands, use [Documentations/22_Project_Command_Reference.md](./Documentations/22_Project_Command_Reference.md).

## Repository Layout

- `Backend/` - .NET microservices backend, gateway, shared building blocks, tests, database scripts, and diagrams
- `Frontend/` - Angular application
- `Documentations/` - project documentation and implementation notes

## Backend Layout

Inside `Backend/` the structure is:

- `src/building-block/` - shared libraries such as `Shared.Events`
- `src/gateway/` - API gateway
- `src/services/` - microservices
- `test/` - unit and integration tests
- `Database/` - SQL scripts
- `Diagrams/` - architecture and design diagrams

Open [Backend/ZyntraPay.slnx](./Backend/ZyntraPay.slnx) for the active backend solution.
