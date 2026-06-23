# HomeControllerHUB Project Report

## Project Overview
HomeControllerHUB is a .NET Core application built using Domain Driven Design (DDD) architecture with CQRS (Command Query Responsibility Segregation) and Mediator patterns. The application appears to be a management system for home controllers/devices with user management, privileges, and establishments.

## Technology Stack
- **.NET Core**: The main framework
- **PostgreSQL**: Database with Entity Framework Core ORM
- **Identity Framework**: User authentication and authorization
- **JWT Authentication**: Token-based authentication
- **FluentValidation**: For command/query validation
- **Swagger/OpenAPI**: API documentation
- **Entity Framework Core**: ORM for data access

## Project Structure
The project is structured into several layers following the DDD approach:

1. **HomeControllerHUB.Api**: 
   - Entry point of the application
   - Contains controllers, middleware, and API configuration
   - Hosts migrations for database schema

2. **HomeControllerHUB.Application**: 
   - Contains application logic
   - Implements CQRS with Commands and Queries
   - Includes validators for commands and queries

3. **HomeControllerHUB.Domain**: 
   - Contains domain entities and business logic
   - Defines entity configurations
   - Core business rules

4. **HomeControllerHUB.Infra**: 
   - Infrastructure concerns
   - Database context and repositories
   - External service integrations
   - Database interceptors for entity processing

5. **HomeControllerHUB.Shared**: 
   - Common utilities and extensions
   - Shared models and DTOs
   - Interface definitions for cross-cutting concerns

6. **HomeControllerHUB.Globalization**: 
   - Internationalization resources
   - Translation services

## Core Domain Entities
1. **ApplicationUser**: 
   - Extends Identity's user model
   - Includes properties like Name, Document, Login
   - Associated with Establishments

2. **Establishment**: 
   - Represents organizations/establishments
   - Has Document, Code, Name, etc.
   - Can be marked as Master establishment

3. **Profile**: 
   - User profiles with permissions
   - Contains Name, Description
   - Associated with Establishments
   - Linked to Privileges through ProfilePrivilege

4. **Privilege**: 
   - System permissions
   - Used to control access to features

5. **ApplicationDomain**: 
   - Domains in the application
   - Organizational structure

6. **ApplicationMenu**: 
   - Menu items in the application
   - Hierarchical structure (parent-child)
   - Associated with Domains

7. **Generic**: 
   - Generic entity for various purposes
   - Type-based classification

## Entity Design
All entities follow these conventions:

1. **Inheritance**: Every entity inherits from `Base` which implements `IBaseEntity`
2. **Properties**: Entities have standard properties like Id, and often include:
   - Name and NormalizedName for searchability 
   - Enable flag for soft deletion
   - Created/Modified timestamps
3. **Normalization**: Entities use the `[Normalized]` attribute for properties that should be normalized
4. **Configuration**: Each entity has a corresponding configuration class in the Configuration folder

## Database Structure
- PostgreSQL database
- Entity Framework Core with code-first migrations
- DbContext configured with interceptors for entity processing
- Identity tables for user authentication
- Indexes on normalized fields for efficient searching
- Code sequences for entity codes (e.g., Establishment.Code)

## CQRS Implementation
The project implements CQRS pattern:

1. **Commands**:
   - Used for create, update, delete operations
   - Each command has a validator
   - Returns appropriate results (id for creation, empty for updates/deletes)
   - Located in `HomeControllerHUB.Application/{EntityPlural}/Commands/{Action}`

2. **Queries**:
   - Used for read operations
   - Can return single entities or collections
   - Support pagination
   - Located in `HomeControllerHUB.Application/{EntityPlural}/Queries/{Action}`

3. **Mediator**:
   - Commands and queries are processed through mediator
   - Provides separation between controllers and handlers

## API Design
1. **Controllers**:
   - One controller per entity type
   - Standard RESTful routes:
     - POST / - Create entity
     - PUT / - Update entity
     - DELETE / - Delete entity
     - GET /list - List all entities (unpaginated)
     - GET / - List entities (paginated)
     - GET /{id} - Get entity by id
   - Controllers delegate business logic to commands/queries

2. **Versioning**:
   - API versioning through URL segments
   - Default version is 1.0

3. **Documentation**:
   - Swagger UI for API documentation
   - Available in development and testing environments

## Authentication and Authorization
1. **JWT Authentication**:
   - Token-based authentication
   - Custom token validation parameters
   - Configured issuer, audience, and secret key

2. **Identity Framework**:
   - Extended with custom user model
   - Role-based authorization
   - Claims-based authorization

3. **Custom Authorization**:
   - Commands and queries use the Authorize attribute from `HomeControllerHUB.Shared.Common`
   - Profile-privilege based permissions

## Error Handling
- Custom error middleware to handle exceptions
- AppError class for structured errors:
  - HTTP status code
  - Error message
  - Optional description
- Globalized error messages

## Implementing New Features
To implement new features in the system, follow these guidelines:

### Creating a New Entity
1. Create the entity class in `HomeControllerHUB.Domain/Entities`:
   - Inherit from `Base`
   - Use singular name for the entity
   - Add properties with proper types
   - Add `[Normalized]` attribute for properties that need normalization
   - Define navigation properties for related entities

2. Create entity configuration in `HomeControllerHUB.Domain/Entities/Configuration`:
   - Define table name, indexes, and constraints
   - Configure relationships with other entities

3. Add the entity to `ApplicationDbContext`:
   - Create a DbSet property
   - Apply the configuration in OnModelCreating

4. Create a migration to update the database schema

### Implementing CRUD Operations
For each entity, create the following components:

1. **DTOs**:
   - Create DTOs that implement `IMapFrom<Entity>` 
   - For paginated results, implement `IPaginatedDto`

2. **Commands**:
   - Create folder structure: `{EntityPlural}/Commands/{Action}`
   - Implement command classes
   - Create validators for each command
   - Return appropriate results (id for creation, void for updates/deletes)

3. **Queries**:
   - Create folder structure: `{EntityPlural}/Queries/{Action}`
   - Implement query classes
   - Create validators if needed (e.g., for search parameters)
   - Return appropriate DTOs or collections

4. **Controller**:
   - Create a controller named `{Entity}Controller`
   - Follow the standard RESTful route naming conventions
   - Use mediator to send commands and queries
   - Apply appropriate authorization

### Example: Creating a New Entity "Device"
1. **Create Entity**:
```csharp
namespace HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Shared.Normalize;

public class Device : Base
{
    public Guid EstablishmentId { get; set; }
    public virtual Establishment Establishment { get; set; } = null!;
    public string? Name { get; set; }
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public bool Enable { get; set; }
    public DeviceType Type { get; set; }
}

public enum DeviceType
{
    Light,
    Door,
    Sensor,
    Other
}
```

2. **Create Configuration**:
```csharp
namespace HomeControllerHUB.Domain.Entities.Configuration;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(255);
        builder.Property(x => x.IpAddress).HasMaxLength(15);
        builder.Property(x => x.MacAddress).HasMaxLength(17);
        
        builder.HasIndex(x => x.NormalizedName);
        
        builder.HasOne(x => x.Establishment)
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

3. **Update ApplicationDbContext**:
```csharp
public DbSet<Device> Devices => Set<Device>();

// In OnModelCreating
modelBuilder.ApplyConfiguration(new DeviceConfiguration());
```

4. **Create Commands and Queries**:
   - Implement CreateDeviceCommand, UpdateDeviceCommand, DeleteDeviceCommand
   - Implement GetDeviceQuery, GetDevicesQuery, GetDeviceListQuery
   - Create validators for each

5. **Create Controller**:
```csharp
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class DeviceController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateDeviceCommand command)
    {
        return await Mediator.Send(command);
    }
    
    [HttpPut]
    public async Task<ActionResult> Update(UpdateDeviceCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete(DeleteDeviceCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceDto>> Get(Guid id)
    {
        return await Mediator.Send(new GetDeviceQuery { Id = id });
    }
    
    [HttpGet]
    public async Task<ActionResult<PaginatedList<DeviceDto>>> Get([FromQuery] GetDevicesQuery query)
    {
        return await Mediator.Send(query);
    }
    
    [HttpGet("list")]
    public async Task<ActionResult<List<DeviceDto>>> GetList()
    {
        return await Mediator.Send(new GetDeviceListQuery());
    }
}
```

## Globalization
- Error messages and responses must be translated using `ISharedResource`
- New messages should be added to `HomeControllerHub.Globalization/Resources`
- Messages are accessed through the resource service

## Best Practices
1. **Error Handling**:
   - Use AppError with appropriate HTTP status code
   - Provide clear error messages that can be translated

2. **Validation**:
   - Create validators for all commands and queries
   - Use FluentValidation rules

3. **Authorization**:
   - Apply the Authorize attribute to commands and queries
   - Check permissions in command/query handlers

4. **Naming Conventions**:
   - Use singular names for entities
   - Use plural names for controller routes and folders
   - Use descriptive names for commands and queries

5. **Database**:
   - Create indexes for frequently queried fields
   - Use normalized fields for text searches
   - Ensure foreign key constraints

6. **Code Organization**:
   - Follow the established folder structure
   - Group related functionality
   - Use interfaces for dependency inversion

## Development Workflow
1. Define the entity and its relationships
2. Configure the entity in the database context
3. Create DTOs for the entity
4. Implement commands and queries with validators
5. Create a controller for the entity
6. Create unit tests for commands and queries
7. Test API endpoints manually or with integration tests

## Deployment
The project includes:
- Dockerfile for containerization
- docker-compose.yml for orchestrating services
- Environment-specific configuration files

## Conclusion
HomeControllerHUB follows a structured approach to software development with clear separation of concerns using DDD and CQRS patterns. This report provides a guide for understanding the existing structure and implementing new features following the established patterns and practices. 