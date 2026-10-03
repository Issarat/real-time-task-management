# Real-time Task Management

A portfolio-ready collaborative Kanban board built with ASP.NET Core 10 MVC, Entity Framework Core, ASP.NET Core Identity, SQL Server, and SignalR.

## Current foundation

- Lightweight Clean Architecture with separate Domain, Application, Infrastructure, and Web projects
- ASP.NET Core MVC application targeting .NET 10
- Controller-based register, login, logout, and access-denied flows backed by ASP.NET Core Identity
- SQL Server LocalDB for local development
- Domain models for projects, members, invite codes, columns, and tasks
- Optimistic concurrency support on tasks
- SignalR hub with project-membership authorization
- EF Core migrations managed by a repository-local `dotnet-ef` tool

## Prerequisites

- .NET SDK 10
- SQL Server LocalDB (installed with the Visual Studio ASP.NET workload)

## Run locally

```powershell
dotnet restore
dotnet tool restore

dotnet tool run dotnet-ef database update `
  --project .\src\RealTimeTaskManagement.Infrastructure\RealTimeTaskManagement.Infrastructure.csproj `
  --startup-project .\src\RealTimeTaskManagement.Web\RealTimeTaskManagement.Web.csproj

dotnet run --project .\src\RealTimeTaskManagement.Web\RealTimeTaskManagement.Web.csproj
```

Open the URL shown in the terminal, then use **Register** to create a local account.

## Project structure

```text
src/
├── RealTimeTaskManagement.Domain/
│   ├── Entities/         Project and Kanban entities
│   └── Enums/            Domain-level enums
├── RealTimeTaskManagement.Application/
│   └── Projects/         Use-case contracts and application abstractions
├── RealTimeTaskManagement.Infrastructure/
│   ├── Identity/         ASP.NET Core Identity user
│   ├── Persistence/      EF Core DbContext, configurations, and migrations
│   └── Projects/         Infrastructure service implementations
└── RealTimeTaskManagement.Web/
    ├── Controllers/      MVC controllers and HTTP routes
    ├── Hubs/             SignalR hubs
    ├── Models/           MVC view models
    ├── Views/            MVC views and shared layouts
    └── wwwroot/          CSS, JavaScript, and static assets
```

Dependencies point inward: `Web -> Application`, `Web -> Infrastructure`,
`Infrastructure -> Application -> Domain`. The Domain project has no dependency
on EF Core, Identity, MVC, or SignalR.

## Planned MVP

1. Project dashboard and project creation
2. Join project through expiring invite codes
3. Three-column Kanban board
4. Task creation, editing, and drag-and-drop
5. Real-time task updates through SignalR
6. Azure App Service and Azure SQL deployment
