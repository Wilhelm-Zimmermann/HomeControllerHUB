# Demo readiness and local run guide

This guide is the final checklist for running and presenting HomeControllerHUB locally. It covers the backend repository at `C:\Projects\HomeControllerHUB` and the frontend repository at `C:\Projects\HomeControllerHub-Front`.

> Keep local credentials and sensor API keys out of Git. Values such as `<demo-sensor-api-key>` below are placeholders, not working credentials.

## Prerequisites

- .NET 9 SDK (`dotnet --version`)
- Docker Desktop with Linux containers (`docker version`)
- Node.js 20.19+ or 22.13+ and npm (`node --version` and `npm --version`)
- Git (`git --version`)

## 1. Start the infrastructure

From `C:\Projects\HomeControllerHUB`, create the external Docker network once:

```bash
docker network inspect home-controller-hub-network || docker network create home-controller-hub-network
```

Then start the services declared in Compose:

```bash
docker compose up -d
docker compose ps
```

| Service | Local address | Notes |
| --- | --- | --- |
| PostgreSQL | `localhost:15432` | PostgreSQL listens on `5432` inside the container. |
| RabbitMQ | `localhost:5672` | AMQP endpoint used by MassTransit. |
| RabbitMQ Management UI | `http://localhost:15672` | The local Compose defaults are `guest` / `guest`; do not reuse them outside local development. |
| Backend API | `http://localhost:6001` | Run with `dotnet run`; the API service in Compose is commented out. |
| Frontend | `http://localhost:5174` | Run from the separate frontend repository; it is not part of this Compose file. |

Wait for PostgreSQL and RabbitMQ to be available before starting the API. `docker compose ps` should show RabbitMQ as healthy.

## 2. Run the backend

From `C:\Projects\HomeControllerHUB`:

```bash
dotnet run --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --launch-profile http
```

Useful URLs in the Development environment:

| Resource | URL |
| --- | --- |
| API base URL | `http://localhost:6001/api/v1` |
| Swagger UI | `http://localhost:6001/swagger` |
| Overall health | `http://localhost:6001/health` |
| Liveness | `http://localhost:6001/health/live` |
| Readiness (PostgreSQL and RabbitMQ) | `http://localhost:6001/health/ready` |

Quick checks:

```bash
curl -i http://localhost:6001/health/live
curl -i http://localhost:6001/health/ready
```

Liveness should succeed as soon as the process is running. Readiness succeeds only when the required dependencies are reachable.

## 3. Run the frontend

In a separate terminal, from `C:\Projects\HomeControllerHub-Front`:

```bash
npm install
npm run dev
```

The frontend runs at `http://localhost:5174`. Set its API version base URL in a local `.env` file if the default does not match your environment:

```dotenv
VITE_API_BASE_URL=http://localhost:6001/api/v1
```

Do not add secrets to Vite variables: every `VITE_*` value is exposed to the browser. The backend CORS configuration must include the exact frontend origin, `http://localhost:5174` (with no path).

## 4. Run the sensor simulator

The safe example is `tools/HomeControllerHUB.SensorSimulator/sensor-simulator.example.json`. Copy it to the Git-ignored local file:

```powershell
Copy-Item tools\HomeControllerHUB.SensorSimulator\sensor-simulator.example.json tools\HomeControllerHUB.SensorSimulator\sensor-simulator.local.json
```

Edit `sensor-simulator.local.json` so each `deviceId` and `apiKey` matches a sensor in your local development database. Never commit real sensor API keys; verify that the local file remains ignored with:

```bash
git check-ignore tools/HomeControllerHUB.SensorSimulator/sensor-simulator.local.json
```

With the API and RabbitMQ running, start the simulator:

```bash
dotnet run --project tools/HomeControllerHUB.SensorSimulator/HomeControllerHUB.SensorSimulator.csproj -- --config tools/HomeControllerHUB.SensorSimulator/sensor-simulator.local.json
```

The simulator should report `queued` responses. Confirm that readings are arriving by checking that:

- the dashboard reading count increases;
- the sensor's last communication and battery values update;
- the sensor detail chart receives new points; and
- the RabbitMQ queue `homecontrollerhub.sensor-telemetry` has consumers and messages do not accumulate indefinitely.

To demonstrate an alert, configure a non-zero `spikeChancePercent` in the local simulator file and use thresholds that match the selected local sensor. A reading outside the thresholds should create an alert after the queued message is consumed.

## 5. Demo flow checklist

- [ ] Start Docker services
- [ ] Confirm PostgreSQL and RabbitMQ are running with `docker compose ps`
- [ ] Start backend API
- [ ] Confirm `/health/live`
- [ ] Confirm `/health/ready`
- [ ] Start frontend
- [ ] Login
- [ ] Open dashboard
- [ ] Start sensor simulator
- [ ] Confirm readings increase
- [ ] Open sensor detail
- [ ] Confirm telemetry chart
- [ ] Trigger threshold alert
- [ ] Confirm alert in dashboard and alerts page
- [ ] Acknowledge alert
- [ ] Confirm the acknowledgement in the audit log
- [ ] Confirm no browser console errors

## 6. Troubleshooting

| Symptom | Check |
| --- | --- |
| Docker is not running | Start Docker Desktop and confirm `docker version` can reach the engine. |
| Docker network is missing | Run `docker network create home-controller-hub-network` once, then retry Compose. |
| A port is already in use | Check ports `15432`, `5672`, `15672`, `6001`, and `5174`; stop the conflicting process or deliberately update both sides of the relevant configuration. |
| RabbitMQ is unhealthy | Run `docker compose logs home-controller-hub-rabbitmq`, wait for its health check, and verify `http://localhost:15672`. |
| PostgreSQL connection failed | Confirm the PostgreSQL container is running, port `15432` is available, and your local connection configuration matches Compose. Do not paste credentials into issues or commits. |
| Frontend request is blocked by CORS | Confirm `VITE_API_BASE_URL` uses `http://localhost:6001/api/v1` and the backend allows the exact origin `http://localhost:5174`; restart both processes after configuration changes. |
| Simulator config is missing | Copy `sensor-simulator.example.json` to `sensor-simulator.local.json`, then supply local sensor values. |
| Simulator gets unauthorized responses | Confirm `deviceId` and `apiKey` belong to the same local sensor. Do not print or share the key. |
| Build reports a locked DLL | Stop the running API and Visual Studio debugging session. If needed, build to a temporary output directory as shown below. |

Temporary-output fallback for a locally locked build:

```powershell
dotnet build HomeControllerHUB.sln --configuration Release -p:OutDir="$env:TEMP\HomeControllerHUB-demo-build\"
```

## 7. Validation commands

Run from `C:\Projects\HomeControllerHUB`:

```bash
dotnet build HomeControllerHUB.sln --configuration Release
dotnet test tests/HomeControllerHUB.Domain.Tests/HomeControllerHUB.Domain.Tests.csproj --configuration Release --no-build
dotnet test tests/HomeControllerHUB.Application.Tests/HomeControllerHUB.Application.Tests.csproj --configuration Release --no-build
dotnet test tests/HomeControllerHUB.Api.IntegrationTests/HomeControllerHUB.Api.IntegrationTests.csproj --configuration Release --no-build
dotnet list HomeControllerHUB.sln package --vulnerable --include-transitive
```

The same build and three test projects run in `.github/workflows/ci.yml` for pushes and pull requests targeting `master` or `main`. Integration tests may require Docker/Testcontainers.

## 8. Before presenting or committing

- [ ] Run the Release build and all three test projects
- [ ] Confirm the five demo ports are available
- [ ] Confirm liveness and readiness are healthy
- [ ] Complete the demo checklist once without manual data repair
- [ ] Check `git diff --check`
- [ ] Review the diff for passwords, JWTs, refresh tokens, API keys, connection strings, personal email addresses, and other sensitive data
- [ ] Confirm `sensor-simulator.local.json` is ignored and not staged
