# HomeControllerHUB API

Backend API for a home automation and IoT monitoring system. The API combines administrative modules, identity and permission management, sensor registration, sensor reading ingestion, alerts, dashboard data, audit logs, health checks, correlation IDs, structured logs, and rate limiting.

This repository contains the backend only. The frontend lives in a separate repository:

```text
C:\proj\personal\HomeControllerHUB-Front
```

## Overview

HomeControllerHUB is built as a portfolio-grade ASP.NET Core API for managing a residential or small-facility IoT environment. It supports two main areas:

- Administrative operations: users, profiles, permissions, establishments, locations, and access control.
- IoT operations: sensors, readings, status updates, alerts, alert acknowledgement, and dashboard summaries.

The project is intentionally structured around real backend concerns: authentication, authorization, persistence, auditability, observability, validation, rate limiting, and automated tests.

## Features

- JWT authentication with refresh tokens.
- ASP.NET Core Identity user management.
- Profile-based and domain/action-based authorization.
- User, profile, permission, and establishment management.
- Hierarchical locations for buildings, floors, rooms, areas, and equipment.
- Sensor registration, update, details, readings, and alerts.
- Public IoT ingestion endpoints for sensor readings, batch readings, and status updates protected by sensor API keys.
- Dashboard summary for active sensors, alerts, readings, and recent activity.
- Alert acknowledgement workflow.
- Audit logs for important commands.
- Sanitized audit metadata for sensitive fields.
- Health checks for liveness and readiness.
- Request correlation through `X-Correlation-ID`.
- Structured request and exception logs.
- Native ASP.NET Core rate limiting.
- Swagger/OpenAPI in Development and Testing.
- Docker Compose support for local PostgreSQL and RabbitMQ.

## Architecture

The solution follows a layered architecture with CQRS-style commands and queries:

```text
HTTP request
  -> Controller
  -> MediatR Command/Query
  -> Pipeline Behaviors
  -> Handler
  -> DbContext / Services
  -> PostgreSQL
```

Main projects:

| Project | Responsibility |
| --- | --- |
| `HomeControllerHUB.Api` | API startup, controllers, middlewares, Swagger, health checks, rate limiting, and migrations. |
| `HomeControllerHUB.Application` | Use cases, commands, queries, handlers, DTOs, validators, and MediatR registration. |
| `HomeControllerHUB.Domain` | Entities, domain models, interfaces, and EF Core entity configuration. |
| `HomeControllerHUB.Infra` | EF Core `ApplicationDbContext`, Identity services, JWT service, email service, interceptors, data initializers, and audit services. |
| `HomeControllerHUB.Shared` | Shared constants, attributes, and utilities used across layers. |
| `HomeControllerHUB.Globalization` | Localized resources and shared messages. |
| `tests` | Application, domain, and API integration tests. |

Important patterns and decisions:

- CQRS with MediatR separates reads and writes.
- Pipeline behaviors enforce authorization and audit logging outside controllers.
- EF Core handles persistence and migrations.
- ASP.NET Core Identity manages users and password/token primitives.
- Domain/action permission names are centralized in shared constants.
- Controllers remain thin and delegate behavior to commands and queries.

## Tech Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 9, ASP.NET Core Web API |
| Database | PostgreSQL, Entity Framework Core, Npgsql |
| Identity | ASP.NET Core Identity |
| Authentication | JWT Bearer, refresh tokens |
| Application flow | MediatR, CQRS, FluentValidation |
| Mapping | Explicit EF Core projections and small DTO mappers |
| Documentation | Swagger / OpenAPI |
| Observability | Health checks, correlation ID middleware, structured logs |
| Tests | xUnit, EF Core InMemory, Testcontainers in existing test suites |
| Messaging | RabbitMQ, MassTransit |
| Local infrastructure | Docker, Docker Compose |
| Email | Mailgun-compatible email service configuration |

## Authentication and Authorization

Authentication uses `POST /api/v1/Users/Token` with form data credentials. A successful login returns an access token and refresh token.

Refresh uses:

```text
POST /api/v1/Users/refresh-token
```

The refresh flow validates the stored refresh token, email confirmation, and the user `Enable` flag. Login currently validates the user login, password, and confirmed email. User enable/disable state is exposed in user administration and is also used in refresh-token validation.

Authorization is enforced through MediatR pipeline behavior. Use cases declare the required domain and action with the shared `Authorize` attribute, and the behavior checks the current user's permissions before the handler executes.

## Backend Configuration

CORS is configured with a single named policy from `Cors:AllowedOrigins`. Local development expects the frontend origin `http://localhost:5174`; override the setting per environment instead of enabling all origins.

EF Core sensitive data logging is disabled by default. It is only enabled when the environment is `Development` and `Database:EnableSensitiveDataLogging` is explicitly set to `true`.

## Domain-Based Permissions

Permissions are modeled around a domain and an action. Examples:

- `User` + `Read`, `Create`, `Update`, `Delete`
- `Profile` + `Read`, `Create`, `Update`, `Delete`
- `Establishment` + `Read`, `Create`, `Update`, `Delete`
- `Location` + `Read`, `Create`, `Update`, `Delete`
- `IoT` + sensor and alert operations
- `AuditLog` + `Read`

Profiles aggregate privileges, users are linked to profiles, and the API evaluates the effective privileges for protected use cases. The platform-level privilege is treated as broad administrative access.

## IoT Monitoring

The IoT module supports:

- Establishments as the organizational boundary.
- Hierarchical locations.
- Sensors with type, model, firmware, thresholds, status, and battery information.
- Sensor readings and batch reading submission.
- Sensor status updates.
- Alerts generated from sensor conditions.
- Alert acknowledgement.
- Dashboard summary for recent monitoring data.

Sensor API keys are used by ingestion/status endpoints and are not exposed by sensor detail responses. This avoids leaking device credentials through regular management screens.

## Sensor Telemetry Ingestion

The current MVP uses HTTP as the telemetry ingress adapter and RabbitMQ/MassTransit for asynchronous sensor reading processing. The queue message does not contain the sensor API key. Future versions can replace the HTTP ingress with MQTT while keeping the same processing pipeline.

Telemetry enters the system through HTTP:

```text
POST /api/v1/SensorReadings/ingest
```

The endpoint is public and does not use user JWT authentication. Each request must include the sensor API key in the `X-Api-Key` header. The API key authenticates only the HTTP ingress request and is validated against the sensor located by `deviceId`; it is never returned in responses, structured logs, audit metadata, or queue messages.

Example payload:

```json
{
  "messageId": "TEMP-SALA-001-20260702T120000Z",
  "deviceId": "TEMP-SALA-001",
  "timestamp": "2026-07-02T12:00:00Z",
  "value": 28.7,
  "unit": "C",
  "batteryLevel": 87,
  "rawData": {
    "firmware": "1.0.3"
  }
}
```

`messageId`, `deviceId`, `value`, and `X-Api-Key` are required. `timestamp` is optional and falls back to `DateTime.UtcNow`. `batteryLevel` is optional and updates the sensor when provided. `rawData` is stored as JSON text for diagnostic data.

After validation, the API sends a `SensorTelemetryReceivedMessage` to the configured RabbitMQ queue through MassTransit and returns `202 Accepted`:

```json
{
  "sensorId": "00000000-0000-0000-0000-000000000000",
  "messageId": "TEMP-SALA-001-20260702T120000Z",
  "status": "Queued"
}
```

If the same message was already processed and stored, the endpoint can return `status: "Duplicate"` instead of publishing another message. If a duplicate is still waiting in the queue, returning `Queued` is acceptable because idempotency is enforced by the consumer.

The main queue is:

```text
homecontrollerhub.sensor-telemetry
```

The queued message contains `SensorId`, `DeviceId`, `MessageId`, `Timestamp`, `Value`, `Unit`, `BatteryLevel`, `RawData`, `ReceivedAt`, and `CorrelationId`. It does not contain API keys, user tokens, authorization headers, passwords, or secrets.

MassTransit configures a durable receive endpoint for the telemetry queue. The retry policy is an interval retry with 3 retries and a 2-second delay between attempts. Delayed redelivery is not used in the current MVP, so no RabbitMQ delayed-message plugin is required.

If the consumer keeps failing after retries, MassTransit moves the message to its standard error queue. Operationally, this is the telemetry DLQ:

```text
homecontrollerhub.sensor-telemetry_error
```

The API currently hosts the MassTransit consumer in the same process. The consumer calls the application processing command, which creates `SensorReading`, updates `LastCommunication`, updates `BatteryLevel` when present, and evaluates `MinThreshold`/`MaxThreshold`. Threshold violations create `SensorAlert` records unless there is already an unacknowledged alert for the same sensor and alert type.

Idempotency is enforced by `messageId` with a unique `SensorId + MessageId` index. The consumer checks for an existing reading before insert and handles unique-index races by returning `status: "Duplicate"` without creating another reading.

## Sensor Health Monitoring

The API runs an automatic sensor health monitor as a hosted service. It periodically sends a MediatR command that checks active sensors and creates technical alerts when a sensor is offline or has low battery.

An active sensor is considered offline when `LastCommunication` has not been set by telemetry yet or is older than `SensorHealthMonitoring:OfflineThresholdMinutes`. A low-battery alert is created when `BatteryLevel` is present and lower than `SensorHealthMonitoring:LowBatteryThreshold`.

The monitor does not create duplicate pending alerts for the same `SensorId` and alert type (`DeviceOffline` or `BatteryLow`). It also does not automatically acknowledge alerts when the sensor recovers.

Configuration:

```json
{
  "SensorHealthMonitoring": {
    "Enabled": true,
    "IntervalSeconds": 60,
    "OfflineThresholdMinutes": 10,
    "LowBatteryThreshold": 20
  }
}
```

These automatic technical alerts are not user actions and do not create audit log entries.

RabbitMQ is part of the readiness check:

```text
GET /health/ready
```

`/health/live` remains process-only and does not depend on the database or broker.

Manual PowerShell check:

```powershell
Invoke-WebRequest -Method Post -Uri "http://localhost:6001/api/v1/SensorReadings/ingest" `
  -ContentType "application/json" `
  -Headers @{ "X-Api-Key" = "<demo-sensor-api-key>" } `
  -Body '{"messageId":"TEMP-SALA-001-001","deviceId":"TEMP-SALA-001","timestamp":"2026-07-02T12:00:00Z","value":29.5,"unit":"C","batteryLevel":87,"rawData":{"firmware":"1.0.3"}}'
```

Local RabbitMQ can be started with Docker Compose:

```bash
docker compose up -d home-controller-hub-rabbitmq
```

Management UI:

```text
http://localhost:15672
```

The default local credentials are `guest` / `guest`, controlled by `RABBITMQ_DEFAULT_USER` and `RABBITMQ_DEFAULT_PASS`. Do not version real broker credentials.

RabbitMQ appsettings:

```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "QueueName": "homecontrollerhub.sensor-telemetry",
    "PublishTimeoutSeconds": 5
  }
}
```

Current limitation: while sensors use HTTP, the API must be online to authenticate and enqueue telemetry. Future roadmap:

- Sensor/ESP32 -> MQTT Broker -> Worker/Consumer -> same processing command -> database/alerts.
- Move the consumer to a dedicated Worker Service when operational needs justify it.
- Keep transport adapters thin so HTTP, MQTT, or background workers reuse the same command behavior.

## Sensor Simulator

The repository includes a local HTTP sensor simulator for development demos and visual telemetry data. It is not a production worker and does not use MQTT yet; the API receives HTTP telemetry and enqueues the readings for asynchronous processing.

The simulator posts readings to:

```text
POST /api/v1/SensorReadings/ingest
```

It uses only the sensor API key through the `X-Api-Key` header. It does not require or send a user JWT token, and it does not print API keys in console output.

Create a local config from the safe example:

```bash
copy tools\HomeControllerHUB.SensorSimulator\sensor-simulator.example.json tools\HomeControllerHUB.SensorSimulator\sensor-simulator.local.json
```

Edit `sensor-simulator.local.json` with the local/demo `deviceId` and sensor `apiKey` values that already exist in your development database. Keep real keys out of Git; `tools/HomeControllerHUB.SensorSimulator/sensor-simulator.local.json` is ignored.

Run the API first, then start the simulator:

```bash
dotnet run --project tools/HomeControllerHUB.SensorSimulator
```

You can also pass an explicit config path:

```bash
dotnet run --project tools/HomeControllerHUB.SensorSimulator -- --config tools/HomeControllerHUB.SensorSimulator/sensor-simulator.local.json
```

Config fields:

| Field | Purpose |
| --- | --- |
| `apiBaseUrl` | API version base URL, for example `http://localhost:6001/api/v1`. |
| `intervalSeconds` | Delay between send cycles. |
| `runForSeconds` | Optional automatic stop time. Omit it to run until `Ctrl+C`. |
| `sensors` | Local/demo sensors to simulate. |
| `deviceId` | Must match an existing sensor. |
| `apiKey` | Must match that sensor's local/demo API key. |
| `minValue` / `maxValue` | Threshold range used to generate occasional alert spikes. |
| `normalMin` / `normalMax` | Normal operating range for regular readings. |
| `spikeChancePercent` | Chance to generate a value below `minValue` or above `maxValue`. |
| `duplicateChancePercent` | Optional chance to resend the last payload to validate `Duplicate`. |
| `batteryStart` | Initial simulated battery percentage. |

Each reading gets a unique `messageId` in the format `{deviceId}-{yyyyMMddHHmmssfff}-{random}`, a UTC timestamp, slowly decreasing battery level, and fake diagnostic `rawData`:

```json
{
  "firmware": "1.0.3",
  "simulated": true,
  "source": "HomeControllerHUB.SensorSimulator"
}
```

Normal values drift inside `normalMin`/`normalMax`. When a spike is generated, the value is sent below `minValue` or above `maxValue` so the backend threshold logic can create alerts after the consumer processes the message. Console output keeps secrets hidden:

```text
[12:00:01] TEMP-SALA-001 -> queued
[12:00:06] TEMP-SALA-001 -> queued
[12:00:11] TEMP-SALA-001 -> duplicate ignored
```

Manual validation flow:

1. Run RabbitMQ with Docker Compose.
2. Open RabbitMQ Management at `http://localhost:15672`.
3. Run the API.
4. Confirm the database has sensors with matching local/demo `deviceId` and `apiKey`.
5. Configure `sensor-simulator.local.json`.
6. Run the simulator and confirm `queued` responses.
7. Confirm messages arrive and are consumed from `homecontrollerhub.sensor-telemetry`.
8. Confirm `SensorReading`, `LastCommunication`, `BatteryLevel`, and threshold alerts are updated.
9. Set `duplicateChancePercent` above zero if you want to verify `Duplicate` responses.
10. Stop RabbitMQ and confirm the API does not return `Queued` while enqueueing is unavailable.
11. If a consumer failure is easy to simulate locally, confirm messages move to `homecontrollerhub.sensor-telemetry_error` after retries.
12. Open the frontend and validate dashboard readings, sensor detail charts, and alert spikes.

## Audit Logs

Audit logging is implemented through `IAuditableCommand` and `AuditLogBehaviour`. Commands that implement this interface can produce audit entries without duplicating logging code inside handlers.

Audited actions include create, update, delete, and alert acknowledgement flows for entities such as:

- `User`
- `Profile`
- `Establishment`
- `Location`
- `Sensor`
- `Alert`

The audit log endpoint is:

```text
GET /api/v1/AuditLogs
```

Available filters include:

- `userId`
- `establishmentId`
- `entityName`
- `entityId`
- `action`
- `createdStart`
- `createdEnd`
- `searchBy`
- standard pagination and ordering from the shared paginated request model

Audit metadata is sanitized before persistence. Sensitive fields such as passwords, tokens, refresh tokens, and sensor API keys are removed from metadata.

## Health Checks

The API exposes:

```text
GET /health
GET /health/live
GET /health/ready
```

- `/health/live` checks whether the application process is running.
- `/health/ready` checks whether the application is ready to receive traffic, including database and RabbitMQ readiness.
- `/health` returns the overall health view.

Examples:

```bash
curl -i http://localhost:6001/health/live
curl -i http://localhost:6001/health/ready
```

## Correlation ID and Structured Logs

Every request uses `X-Correlation-ID`.

- If the client sends the header, the API preserves it.
- If the client does not send it, the API generates a GUID.
- The same value is returned in the response header.
- Request logs and exception logs include the correlation ID.

Example:

```bash
curl -i -H "X-Correlation-ID: test-correlation-123" http://localhost:6001/api/v1/Dashboard/summary
```

Request logs include method, path, status code, elapsed time, user when available, remote IP address, and correlation ID. Logs should not include request bodies, passwords, tokens, refresh tokens, or API keys.

## Rate Limiting

The API uses native ASP.NET Core rate limiting with fixed one-minute windows.

| Policy | Limit | Partition key | Main usage |
| --- | --- | --- | --- |
| `AuthenticatedPolicy` | 100 requests/minute | authenticated user id, with IP fallback | default API controller policy |
| `AuthPolicy` | 10 requests/minute | IP address | public authentication and password-token endpoints |
| `SensitivePolicy` | 20 requests/minute | authenticated user id, with IP fallback | destructive or sensitive actions |
| `SensorIngestionPolicy` | 60 requests/minute | IP address | public sensor telemetry ingestion |

`AuthenticatedPolicy` is applied at `ApiControllerBase`, so regular API controllers inherit it by default. `AuthPolicy` is applied to:

- `POST /api/v1/Users/Token`
- `POST /api/v1/Users/refresh-token`
- `POST /api/v1/Users/forgot-password`
- `POST /api/v1/Users/reset-password-with-token`

`SensitivePolicy` is applied to password reset and destructive actions for users, profiles, establishments, locations, and sensors.

Health checks are not globally rate limited.

When a limit is exceeded, the API returns `429 Too Many Requests`, includes `X-Correlation-ID`, includes `Retry-After` when available, and writes:

```json
{
  "title": "Too many requests",
  "message": "Voce fez muitas requisicoes em pouco tempo. Tente novamente em alguns instantes.",
  "statusCode": 429,
  "correlationId": "..."
}
```

Manual PowerShell check:

```powershell
1..12 | ForEach-Object {
  Invoke-WebRequest -Method Post -Uri "http://localhost:6001/api/v1/Users/Token" -Body @{ UserName = "test@example.com"; Password = "invalid" } -Headers @{ "X-Correlation-ID" = "manual-rate-limit-test" } -SkipHttpErrorCheck
}
```

## API Modules

Useful module entry points:

| Module | Main endpoints |
| --- | --- |
| Authentication | `POST /api/v1/Users/Token`, `POST /api/v1/Users/refresh-token` |
| Current user | `GET /api/v1/Users/current` |
| Users | `GET /api/v1/Users/list`, `GET /api/v1/Users/{id}`, `POST /api/v1/Users`, `PUT /api/v1/Users/{id}`, `DELETE /api/v1/Users/{id}` |
| Profiles | `GET /api/v1/Profiles`, `GET /api/v1/Profiles/list`, `GET /api/v1/Profiles/{id}`, `POST /api/v1/Profiles`, `PUT /api/v1/Profiles/{id}`, `DELETE /api/v1/Profiles/{id}` |
| Privileges | `GET /api/v1/Privilege/list` |
| Establishments | `GET /api/v1/Establishment`, `GET /api/v1/Establishment/list`, `GET /api/v1/Establishment/{id}`, `POST /api/v1/Establishment`, `PUT /api/v1/Establishment/{id}`, `DELETE /api/v1/Establishment/{id}` |
| Locations | `GET /api/v1/Locations`, `GET /api/v1/Locations/list`, `GET /api/v1/Locations/hierarchical`, `GET /api/v1/Locations/{id}`, `POST /api/v1/Locations`, `PUT /api/v1/Locations`, `DELETE /api/v1/Locations` |
| Sensors | `GET /api/v1/Sensors`, `GET /api/v1/Sensors/list`, `GET /api/v1/Sensors/{id}`, `GET /api/v1/Sensors/{id}/readings`, `GET /api/v1/Sensors/{id}/alerts`, `POST /api/v1/Sensors`, `PUT /api/v1/Sensors`, `DELETE /api/v1/Sensors` |
| Sensor readings | `POST /api/v1/SensorReadings/ingest` |
| Sensor data | legacy public IoT ingestion/status endpoints in `SensorDataController` |
| Alerts | `GET /api/v1/Alerts`, `PATCH /api/v1/Alerts/{id}/acknowledge` |
| Dashboard | `GET /api/v1/Dashboard/summary` |
| Audit logs | `GET /api/v1/AuditLogs` |
| Health | `GET /health`, `GET /health/live`, `GET /health/ready` |

Swagger is available in Development and Testing:

```text
http://localhost:6001/swagger
```

## Database

The API uses PostgreSQL through Entity Framework Core. Migrations are stored in `src/HomeControllerHUB.Api/Migrations`, while `ApplicationDbContext` is implemented in the infrastructure layer.

During startup, the API applies migrations when the provider is relational and not the in-memory provider used by tests.

Local PostgreSQL is configured by `docker-compose.yml` on port `15432`.

Create the required Docker network once:

```bash
docker network create home-controller-hub-network
```

Start PostgreSQL:

```bash
docker compose up -d home-controller-hub-postgres
```

Apply migrations manually when needed:

```bash
dotnet ef database update --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --startup-project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --context ApplicationDbContext
```

## Tests

Run all tests:

```bash
dotnet test HomeControllerHUB.sln
```

Run only API integration tests:

```bash
dotnet test tests/HomeControllerHUB.Api.IntegrationTests/HomeControllerHUB.Api.IntegrationTests.csproj --no-restore
```

The test suite includes application tests, domain tests, and API integration tests for health checks, correlation ID, and rate limiting. Some test suites use Docker/Testcontainers and require Docker to be running.

## Running Locally

Prerequisites:

- .NET SDK 9.0.
- Docker Desktop or Docker Engine.
- PostgreSQL through Docker Compose or a compatible local PostgreSQL instance.
- Optional: `dotnet-ef` for manual migration commands.

Restore:

```bash
dotnet restore HomeControllerHUB.sln
```

Build:

```bash
dotnet build HomeControllerHUB.sln
```

Run:

```bash
dotnet run --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --launch-profile http
```

Default local URL:

```text
http://localhost:6001
```

## Environment Variables

The API reads configuration from:

```text
src/HomeControllerHUB.Api/appsettings.json
src/HomeControllerHUB.Api/appsettings.Development.json
src/HomeControllerHUB.Api/appsettings.Testing.json
```

Important configuration areas:

- `ConnectionStrings:Npgsql`
- `ApplicationSettings:JwtSettings`
- `ApplicationSettings:HostSettings`
- `ApplicationSettings:SwaggerSettings`
- `ApplicationSettings:InitializeDataBase`
- `EmailSettings:MailgunApiKey`
- `EmailSettings:MailgunDomain`
- `EmailSettings:FrontendUrl`
- `EmailSettings:SenderEmail`
- `EmailSettings:SenderName`
- `RabbitMq:Host`
- `RabbitMq:Port`
- `RabbitMq:VirtualHost`
- `RabbitMq:Username`
- `RabbitMq:Password`
- `RabbitMq:QueueName`
- `RabbitMq:PublishTimeoutSeconds`
- `SensorHealthMonitoring:Enabled`
- `SensorHealthMonitoring:IntervalSeconds`
- `SensorHealthMonitoring:OfflineThresholdMinutes`
- `SensorHealthMonitoring:LowBatteryThreshold`

Do not commit real database credentials, JWT secrets, Mailgun keys, refresh tokens, sensor API keys, or production URLs.

## Useful Endpoints

```bash
curl -i http://localhost:6001/health/live
curl -i http://localhost:6001/health/ready
curl -i -H "X-Correlation-ID: test-correlation-123" http://localhost:6001/api/v1/Dashboard/summary
```

Token request example:

```bash
curl -i -X POST http://localhost:6001/api/v1/Users/Token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin@example.com" \
  -d "password=change-me"
```

## Project Structure

```text
.
|-- src/
|   |-- HomeControllerHUB.Api/
|   |-- HomeControllerHUB.Application/
|   |-- HomeControllerHUB.Domain/
|   |-- HomeControllerHUB.Globalization/
|   |-- HomeControllerHUB.Infra/
|   `-- HomeControllerHUB.Shared/
|-- tests/
|-- docs/
|-- docker-compose.yml
|-- Dockerfile
`-- HomeControllerHUB.sln
```

## Security Notes

- JWT secret keys and refresh tokens must be treated as secrets.
- Sensor API keys are device credentials and must not be exposed in management responses.
- Audit metadata is sanitized to avoid storing sensitive values.
- Backend authorization is authoritative; frontend permission checks are only a UX layer.
- Public auth endpoints are rate limited by IP.
- Destructive and sensitive endpoints have a stricter policy than regular authenticated endpoints.
- Establishment deletion currently behaves as logical deactivation by setting `Enable = false`.
- User and profile deletes remove records and related links according to the current handlers.

## Complementary Documentation

The `docs/` folder contains additional project notes:

- `docs/Frontend_API_Documentation.md`
- `docs/IoT_Implementation_Guide.md`
- `docs/ProjectReport.md`
- `docs/ProjDoc.md`
- `docs/PROJECT_REPORT.md`

## Roadmap

- Export audit logs to CSV.
- Add charts for sensor reading history.
- Add OpenTelemetry tracing.
- Add deployment pipeline.
- Add Docker production profile.
- Review authentication enable/disable behavior for login consistency.
- Expand integration tests for authenticated rate-limit partitioning.

## Author

Wilhelm Henrique Zimmermann

GitHub: [Wilhelm-Zimmermann](https://github.com/Wilhelm-Zimmermann)
