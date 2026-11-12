# BankApi

A banking transaction API built with C# and ASP.NET Core (.NET 8). It handles customers, accounts, deposits, withdrawals and transfers, with role-based access for admins, tellers and customers.

Built during my Technical Internship at Wema Bank (Automation & Innovation), Aug–Nov 2025.

## Tech stack

- **C# / ASP.NET Core 8** Web API
- **Entity Framework Core** with **SQL Server** (code-first migrations)
- **JWT authentication** with role-based authorization (Admin, Teller, Customer)
- **Redis** for caching the dashboard summary and limiting repeated failed logins
- **Serilog** structured logging (console + rolling file)
- **Swagger / OpenAPI** with a JWT "Authorize" button
- Layered **Repository / Service** architecture

## Endpoints (27)

| Area | Endpoints |
|---|---|
| **Auth** (3) | `POST /api/auth/register` · `POST /api/auth/login` · `GET /api/auth/profile` |
| **Accounts** (8) | list, get by id, get balance, create, get by user, update, delete, search by account number |
| **Transactions** (9) | deposit, withdraw, transfer, get by account / transaction / transfer / user, list all, filter by type, direction and date range |
| **Users** (6) | list, get by id, update, delete, list customers, list tellers |
| **Dashboard** (1) | `GET /api/dashboard/summary` (Admin, Teller) |

Each endpoint is restricted by role. For example, deposits and withdrawals are Admin/Teller only, while customers can transfer from and view only their own accounts.

## Running it locally

**Requirements:** .NET 8 SDK, SQL Server (LocalDB or Express), Redis on `localhost:6379`.

```bash
# 1. Restore packages
dotnet restore

# 2. Set the JWT signing key (kept out of source control)
dotnet user-secrets set "Jwt:Key" "<a long random string, 32+ characters>"

# 3. Create the database
dotnet ef database update

# 4. Run
dotnet run
```

Then open `http://localhost:5299/swagger`. Register a user, log in, and paste the token into **Authorize** as `Bearer <token>`.

If your SQL Server instance isn't `localhost\SQLEXPRESS`, update `ConnectionStrings:DefaultConnection` in `appsettings.json`.

## Project structure

```
Controllers/     API endpoints (Accounts, Auth, Dashboard, Transactions, Users)
Services/        Business logic + Redis cache service
Repositories/    Data access over EF Core
Models/  DTO/    Entities and request/response shapes
Db/              AppDbContext
Helpers/         JWT, password hashing, account number generation
Migrations/      EF Core migrations
```

## Roadmap

- Unit tests for the services
- Separate command and query handlers (CQRS)
- Docker Compose for SQL Server + Redis
