<h1 align="center">HomeControllerHUB</h1>

<p align="center">
  Backend API for home automation, environment management, IoT sensors, users, establishments, and monitoring data.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core" />
  <img src="https://img.shields.io/badge/PostgreSQL-Database-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker" />
  <img src="https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black" alt="Swagger" />
  <img src="https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge" alt="Status" />
</p>

<p align="center">
  <a href="#overview">Overview</a> •
  <a href="#main-features">Features</a> •
  <a href="#tech-stack">Tech stack</a> •
  <a href="#architecture">Architecture</a> •
  <a href="#running-the-project">Running</a> •
  <a href="#tests">Tests</a>
</p>

---

## Overview

**HomeControllerHUB** is a REST API built with .NET to support the administration of a home automation and IoT monitoring platform.

The project combines administrative features, such as user, profile, privilege, and establishment management, with IoT-related capabilities, including location registration, sensor registration, reading ingestion, status updates, and alert support.

This repository contains only the backend of the solution. The API was designed to be consumed by frontend applications, administrative systems, and IoT devices.

## Main features

* Authentication with ASP.NET Core Identity and JWT.
* Access control based on profiles, privileges, domains, and actions.
* User, profile, and establishment management.
* Hierarchical location and IoT sensor registration.
* Individual and batch sensor reading ingestion.
* Sensor status update registration.
* Support for alerts based on thresholds and low battery.
* PostgreSQL persistence with Entity Framework Core.
* API documentation with Swagger/OpenAPI.
* Message globalization support.
* Automated tests with xUnit and Testcontainers.
* Docker Compose support for local development.

## Tech stack

| Category       | Technologies                                 |
| -------------- | -------------------------------------------- |
| Backend        | .NET 9, ASP.NET Core Web API                 |
| Database       | PostgreSQL, Entity Framework Core, Npgsql    |
| Authentication | ASP.NET Core Identity, JWT Bearer            |
| Architecture   | MediatR, CQRS, FluentValidation, AutoMapper  |
| Documentation  | Swagger / OpenAPI                            |
| Tests          | xUnit, Moq, FluentAssertions, Testcontainers |
| Infrastructure | Docker, Docker Compose                       |
| Integrations   | Mailgun via HttpClient                       |

## Architecture

The project follows a layered architecture, separating HTTP entry points, use cases, domain rules, infrastructure concerns, and shared resources.

Main flow:

```text
Controller -> Command/Query -> Handler -> Services/DbContext -> PostgreSQL
```

Main layers:

| Project                           | Responsibility                                                                               |
| --------------------------------- | -------------------------------------------------------------------------------------------- |
| `HomeControllerHUB.Api`           | API entry point, controllers, middlewares, Swagger, API versioning, and application startup. |
| `HomeControllerHUB.Application`   | Use cases organized into commands, queries, handlers, DTOs, and validations.                 |
| `HomeControllerHUB.Domain`        | Entities, models, interfaces, mappings, and domain configurations.                           |
| `HomeControllerHUB.Infra`         | Persistence, services, interceptors, data initializers, and external integrations.           |
| `HomeControllerHUB.Shared`        | Shared constants, attributes, utilities, and helpers.                                        |
| `HomeControllerHUB.Globalization` | Localization resources and services.                                                         |
| `tests`                           | Automated test projects.                                                                     |

## Repository structure

```text
.
├── src/
│   ├── HomeControllerHUB.Api/
│   ├── HomeControllerHUB.Application/
│   ├── HomeControllerHUB.Domain/
│   ├── HomeControllerHUB.Globalization/
│   ├── HomeControllerHUB.Infra/
│   └── HomeControllerHUB.Shared/
├── tests/
├── docs/
├── Dockerfile
├── docker-compose.yml
└── HomeControllerHUB.sln
```

## Prerequisites

* .NET SDK 9.0.
* Docker Desktop or Docker Engine.
* Local PostgreSQL or PostgreSQL through Docker Compose.
* `dotnet-ef`, if you want to manage migrations manually.

Optional Entity Framework CLI installation:

```bash
dotnet tool install --global dotnet-ef
```

## Local configuration

The main application settings are located in:

```text
src/HomeControllerHUB.Api/appsettings.json
src/HomeControllerHUB.Api/appsettings.Development.json
src/HomeControllerHUB.Api/appsettings.Testing.json
```

For local development, the project uses PostgreSQL on port `15432`, as defined in `docker-compose.yml`.

Because the compose file uses an external network, create it before the first execution:

```bash
docker network create home-controller-hub-network
```

For real environments, use environment variables, Secret Manager, or another secure mechanism for connection strings, JWT keys, database credentials, and email settings.

## Running the project

Start PostgreSQL with Docker Compose:

```bash
docker compose up -d home-controller-hub-postgres
```

Restore dependencies:

```bash
dotnet restore HomeControllerHUB.sln
```

Build the solution:

```bash
dotnet build HomeControllerHUB.sln
```

Run the API:

```bash
dotnet run --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --launch-profile http
```

By default, the `http` launch profile runs the API at:

```text
http://localhost:6001
```

## Database and migrations

The project uses Entity Framework Core Migrations. The migrations are located in the `HomeControllerHUB.Api` project, while the `ApplicationDbContext` is part of the `HomeControllerHUB.Infra` layer.

During application startup, migrations are applied automatically according to the current configuration:

```csharp
dbContext.Database.Migrate();
```

To apply migrations manually:

```bash
dotnet ef database update --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --startup-project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --context ApplicationDbContext
```

The project also includes development data initialization when the `ApplicationSettings:InitializeDataBase` configuration is enabled.

### Development demo data

When `ASPNETCORE_ENVIRONMENT=Development` and `ApplicationSettings:InitializeDataBase` is enabled, the API also runs `DevelopmentDataInitializer`.

The initializer is idempotent: if any sensor with a `demo-` device id already exists, it skips demo data creation. It prefers the existing `WillHome` establishment, then the first active non-master establishment, and only creates `WillHome Demo` if no suitable establishment exists.

Demo data includes:

* A location hierarchy with `Casa`, floors, rooms, garage, external area, and garden.
* 8 sensors with device ids such as `demo-temp-living-room`, `demo-smoke-kitchen`, and `demo-water-garden`.
* Recent and older sensor readings for dashboard and detail screens.
* Sensor status updates with battery, firmware, and signal metadata.
* Pending and acknowledged alerts, including low battery, threshold, smoke, gas, humidity, and offline scenarios.

To populate a local database, start PostgreSQL and run the API in Development:

```bash
dotnet run --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --launch-profile http
```

To recreate the demo data, remove the demo sensors whose `DeviceId` starts with `demo-` and restart the API. Related readings, status updates, and alerts are removed through sensor cascades or can be cleared explicitly from their tables.

## Tests

Run all tests:

```bash
dotnet test HomeControllerHUB.sln
```

Some tests use Testcontainers with PostgreSQL. To run this test suite, Docker must be running and accessible in the local environment.

## API documentation

When the API is running in the `Development` or `Testing` environments, Swagger documentation is available at:

```text
http://localhost:6001/swagger
```

This README provides only a high-level overview of the project. Endpoint details, contracts, and integration guidance are available in the complementary documentation.

## Complementary documentation

The `docs/` folder contains more detailed project materials:

* `docs/Frontend_API_Documentation.md`: frontend integration guide.
* `docs/IoT_Implementation_Guide.md`: IoT domain guide.
* `docs/ProjectReport.md`: general project report.
* `docs/ProjDoc.md`: architecture, entities, and execution documentation.
* `docs/PROJECT_REPORT.md`: detailed analysis of the structure, flows, and technical points.

## Project status

Project in progress, developed for studying and building a backend solution focused on home automation, environment administration, and IoT monitoring.

## Author

**Wilhelm Henrique Zimmermann**

GitHub: [Wilhelm-Zimmermann](https://github.com/Wilhelm-Zimmermann)
