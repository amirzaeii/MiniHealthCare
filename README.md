# MiniHealthCare

MiniHealthCare is a small ASP.NET Core web API that implements core healthcare management functionality: patients, doctors, appointments, prescriptions, and medical records. The project is organized as a minimal, layered API with Entity Framework Core for data access, configuration under `HealthCare/`, and endpoint routing under `HealthCare/Endpoints`.

## Summary

- Purpose: Provide a lightweight backend API for healthcare operations (appointments, patient/doctor records, prescriptions).
- Main pieces: `HealthCare` API project, EF Core migrations, `Services` implementations for business logic, and `Endpoints` for HTTP routes.
- Tech stack: .NET 10 (net10.0), ASP.NET Core Web API, Entity Framework Core, C#.

## Features

- CRUD for Patients, Doctors, Appointments, Medical Records, Prescriptions
- Tenant isolation filter and exception handling middleware
- JWT-based authentication flows and token service
- EF Core migrations included (Migrations folder)

## Prerequisites

- .NET 10 SDK (install from https://dotnet.microsoft.com)
- (Optional) `dotnet-ef` tool for applying migrations: `dotnet tool install --global dotnet-ef`
- A database (by default configure connection string in `HealthCare/appsettings.json`)

## Quickstart — run locally

1. Clone the repository:

```bash
git clone <repo-url>
cd MiniHealthCare
```

2. Restore and build:

```bash
dotnet restore
dotnet build
```

3. Configure the database connection:

- Edit `HealthCare/appsettings.json` (or `appsettings.Development.json`) and set the `ConnectionStrings:Default` value to your database (e.g., SQL Server, PostgreSQL, or SQLite).

4. Apply EF Core migrations (if using `dotnet-ef`):

```bash
dotnet ef database update --project HealthCare/HealthCare.csproj --startup-project HealthCare/HealthCare.csproj
```

5. Run the API:

```bash
dotnet run --project HealthCare/HealthCare.csproj
```
