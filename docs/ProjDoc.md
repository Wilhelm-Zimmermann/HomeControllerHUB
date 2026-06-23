# HomeControllerHUB Project Documentation

## 1. Overview

HomeControllerHUB is a backend system designed for home and building automation, with a strong focus on IoT device management. It provides a centralized platform for managing establishments, locations (like rooms or floors), users, and a wide array of sensors.

The system is capable of receiving and processing data from IoT devices (e.g., temperature, humidity, motion sensors), managing user access and permissions, handling device status updates, and triggering alerts based on predefined thresholds. It is a multi-tenant system, with data partitioned by `Establishment`.

## 2. Architecture & Design Patterns

The project is structured following the principles of **Clean Architecture**, which promotes a separation of concerns and creates a loosely coupled, maintainable, and testable system.

### Project Layers

-   **`HomeControllerHUB.Domain`**: The core of the application. It contains all domain entities, value objects, and the most critical business rules. It has no dependencies on any other layer.
-   **`HomeControllerHUB.Application`**: This layer contains the application-specific business logic. It orchestrates the domain entities to perform use cases. It depends on the `Domain` layer but not on the `Infrastructure` or `Api` layers.
-   **`HomeControllerHUB.Infrastructure`**: Implements external concerns, such as databases, file systems, and third-party services. It contains implementations for interfaces defined in the `Application` layer (like repositories). It depends on the `Application` and `Domain` layers.
-   **`HomeControllerHUB.Api`**: The presentation layer. This is an ASP.NET Core Web API that exposes the application's functionality to the outside world via RESTful endpoints. It depends on the `Application` and `Infrastructure` layers.
-   **`HomeControllerHUB.Shared`**: Contains cross-cutting concerns and shared utilities used across all layers, such as normalization logic and constants.
-   **`HomeControllerHUB.Globalization`**: Manages internationalization (i18n) and localization (l10n) using resource files.

### Key Design Patterns

-   **CQRS (Command Query Responsibility Segregation)**: The application separates read operations (Queries) from write operations (Commands). This is implemented using the **MediatR** library, where each API endpoint dispatches either a command or a query to its respective handler in the `Application` layer.
-   **Dependency Injection (DI)**: Heavily used throughout the application, configured in `Program.cs` and `ConfigureServices.cs` files. Services and repositories are injected into the classes that need them, promoting loose coupling.
-   **Repository Pattern**: Abstracted through Entity Framework Core's `DbContext`. The `ApplicationDbContext` acts as a Unit of Work and a repository for the domain entities.
-   **Middleware**: Custom middleware (`ErrorHandlingMiddleware`) is used to create a centralized exception handling pipeline for the API.
-   **FluentValidation**: Used for validating all incoming requests (Commands and Queries) to ensure data integrity before they hit the core business logic.

## 3. Technology Stack

-   **Backend Framework**: .NET 9.0
-   **API**: ASP.NET Core Web API
-   **Database**: PostgreSQL
-   **ORM**: Entity Framework Core (EF Core)
-   **Key Libraries**:
    -   **MediatR**: For implementing the CQRS pattern.
    -   **FluentValidation**: For request validation.
    -   **Asp.Versioning.Mvc**: For API versioning.
    -   **Swashbuckle**: For Swagger/OpenAPI documentation.
    -   **Microsoft.AspNetCore.Identity**: For user authentication and authorization.
-   **Containerization**: Docker & Docker Compose

## 4. Database Schema

The following tables are defined by the entities in the `HomeControllerHUB.Domain/Entities` folder.

<details>
<summary>Expand to see all tables</summary>

### `ApplicationDomain`
Stores application domains or modules.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `Name` | `string` | Required |
| `Description` | `string` | Nullable |
| `Enable` | `bool` | Required |

### `ApplicationMenu`
Stores navigation menu items.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `ParentId` | `Guid?` | FK to `ApplicationMenu` |
| `DomainId` | `Guid?` | FK to `ApplicationDomain` |
| `Name` | `string` | Nullable |
| `Description` | `string` | Nullable |
| `IconClass` | `string` | Nullable |
| `Link` | `string` | Nullable |
| `Target` | `string` | Nullable |
| `Order` | `int` | |
| `Enable` | `bool` | |

### `ApplicationUser` (IdentityUser)
Stores user account information.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `EstablishmentId` | `Guid` | FK to `Establishment` |
| `Name` | `string` | Nullable |
| `Code` | `string` | Nullable |
| `Login` | `string` | Nullable |
| `Document` | `string` | Nullable |
| `Enable` | `bool` | |
| `EmailConfirmationToken` | `string` | Nullable |
| `PasswordConfirmationToken` | `string` | Nullable |
| `...` | `...` | (Inherits from IdentityUser) |

### `Establishment`
Represents a physical or logical tenant, like a building or company.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `Code` | `string` | Nullable |
| `Name` | `string` | Nullable |
| `SiteName` | `string` | Nullable |
| `Document` | `string` | Required |
| `Enable` | `bool` | |
| `IsMaster` | `bool` | |
| `SubscriptionPlanId` | `Guid?` | FK to `SubscriptionPlan` |
| `SubscriptionEndDate` | `DateTime?` | Nullable |

### `Generic`
For generic lookup data.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `Identifier` | `string` | |
| `Code` | `string` | Nullable |
| `Value` | `string` | Nullable |
| `DisplayOrder` | `int?` | Nullable |
| `Enable` | `bool` | |

### `Location`
Represents a location within an establishment (e.g., floor, room).
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `EstablishmentId` | `Guid` | FK to `Establishment` |
| `Name` | `string` | Required |
| `Description` | `string` | Nullable |
| `Type` | `enum (LocationType)` | |
| `ParentLocationId` | `Guid?` | FK to `Location` |

### `Privilege`
Defines specific permissions in the system.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `Name` | `string` | Required |
| `Description` | `string` | Nullable |
| `Actions` | `string` | Nullable |
| `Enable` | `bool` | |
| `DomainId` | `Guid` | FK to `ApplicationDomain` |
| `EstablishmentId` | `Guid` | FK to `Establishment` |

### `Profile`
A collection of privileges that can be assigned to users.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `EstablishmentId` | `Guid` | FK to `Establishment` |
| `Name` | `string` | Nullable |
| `Description` | `string` | Nullable |
| `Enable` | `bool` | |

### `ProfilePrivilege`
Junction table between `Profile` and `Privilege`.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `ProfileId` | `Guid` | FK to `Profile` |
| `PrivilegeId` | `Guid` | FK to `Privilege` |

### `Sensor`
Represents an IoT sensor device.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `EstablishmentId` | `Guid` | FK to `Establishment` |
| `LocationId` | `Guid` | FK to `Location` |
| `Name` | `string` | Required |
| `DeviceId` | `string` | Required, Unique |
| `Type` | `enum (SensorType)` | |
| `Model` | `string` | Required |
| `FirmwareVersion` | `string` | Nullable |
| `ApiKey` | `string` | Nullable |
| `MinThreshold` | `double?` | Nullable |
| `MaxThreshold` | `double?` | Nullable |
| `IsActive` | `bool` | |
| `LastCommunication` | `DateTime` | |
| `BatteryLevel` | `double?` | Nullable |

### `SensorAlert`
Stores alerts generated by sensors.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `SensorId` | `Guid` | FK to `Sensor` |
| `Type` | `enum (AlertType)` | |
| `Message` | `string` | Required |
| `Timestamp` | `DateTime` | |
| `IsAcknowledged` | `bool` | |
| `AcknowledgedAt` | `DateTime?` | Nullable |
| `AcknowledgedById` | `Guid?` | FK to `ApplicationUser` |

### `SensorReading`
Stores time-series data from sensors.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `SensorId` | `Guid` | FK to `Sensor` |
| `Timestamp` | `DateTime` | |
| `Value` | `double` | |
| `Unit` | `string` | Nullable |
| `RawData` | `string` | Nullable |
| `Metadata` | `Dictionary<string, string>` | (jsonb) |

### `SensorStatusUpdate`
Stores status updates from sensors (e.g., battery, connectivity).
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `SensorId` | `Guid` | FK to `Sensor` |
| `Timestamp` | `DateTime` | |
| `Status` | `string` | Nullable |
| `BatteryLevel` | `double?` | Nullable |
| `SignalStrength` | `string` | Nullable |
| `Metadata` | `Dictionary<string, string>` | (jsonb) |

### `SubscriptionPlan`
Defines subscription tiers and their limits.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `Name` | `string` | Required, Unique |
| `Description` | `string` | Nullable |
| `Price` | `decimal` | |
| `MaxSensors` | `int` | |
| `DataRetentionDays` | `int` | |
| `AlertsPerMonth` | `int` | |
| `IncludesReporting` | `bool` | |
| `IncludesApiAccess` | `bool` | |

### `UserEstablishment`
Junction table between `ApplicationUser` and `Establishment`.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `UserId` | `Guid` | FK to `ApplicationUser` |
| `EstablishmentId` | `Guid` | FK to `Establishment` |

### `UserProfile`
Junction table between `ApplicationUser` and `Profile`.
| Field | Type | Constraints |
| :--- | :--- | :--- |
| `Id` | `Guid` | Primary Key |
| `UserId` | `Guid` | FK to `ApplicationUser` |
| `ProfileId` | `Guid` | FK to `Profile` |

</details>

## 5. API Endpoints

The API is versioned, with the current version being `v1`. All endpoints are prefixed with `/api/v1/`.

-   **`EstablishmentController`**: `POST, PUT, DELETE, GET /api/v1/Establishment`
    -   Manages CRUD operations for establishments.
-   **`GenericController`**: `GET /api/v1/Generic/list`
    -   Retrieves generic lookup data.
-   **`LocationsController`**: `POST, PUT, DELETE, GET /api/v1/Locations`
    -   Manages CRUD for locations, including hierarchical data.
-   **`MenuController`**: `GET /api/v1/Menu/list`
    -   Retrieves menu items for UI navigation.
-   **`PrivilegeController`**: `GET /api/v1/Privilege/list`
    -   Retrieves a list of available permissions.
-   **`ProfilesController`**: `POST, PUT, DELETE, GET /api/v1/Profiles`
    -   Manages CRUD for user profiles (permission sets).
-   **`SensorDataController`**: `POST /api/v1/SensorData/readings`, `POST /api/v1/SensorData/status`
    -   Publicly accessible endpoints for IoT devices to submit data and status updates.
-   **`SensorsController`**: `POST, PUT, DELETE, GET /api/v1/Sensors`
    -   Manages CRUD for sensors and retrieves their readings and alerts.
-   **`UsersController`**: `POST, PUT, DELETE, GET /api/v1/Users`
    -   Manages user accounts, authentication (`/token`), password resets, and email confirmation.

## 6. Getting Started

### Prerequisites

-   .NET 9 SDK
-   Docker (recommended)
-   A PostgreSQL instance.

### Running the Application

#### Using Docker (Recommended)

The `docker-compose.yml` file is configured to set up the PostgreSQL database.

1.  **Start the database:**
    ```bash
    docker-compose up -d home-controller-hub-postgres
    ```
2.  **Run the API:**
    You can run the API from your IDE (like Rider or Visual Studio) or via the .NET CLI. The application is configured to connect to the Dockerized database when using the `Testing` environment.
    ```bash
    dotnet run --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --launch-profile "Testing"
    ```
    The API will be available at `http://localhost:5000` (or as configured in `launchSettings.json`).

#### Using .NET CLI

1.  **Ensure you have a running PostgreSQL database.**
2.  **Update `appsettings.Development.json`**:
    Modify the `DefaultConnection` string to point to your PostgreSQL instance.
3.  **Run the migrations**:
    ```bash
    dotnet ef database update --project src/HomeControllerHUB.Api
    ```
4.  **Run the application**:
    ```bash
    dotnet run --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj
    ```

### Running Tests

To run the unit and integration tests for the entire solution, navigate to the root directory and run:

```bash
dotnet test
```
