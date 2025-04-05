# General Informations
- It is not allowed to modify this Readme.md file
- Do not ask permition to create new folders, simpy do it all the code written here will be revised and if needs improvement it will be suggested
- This app uses the "Domain Driven Design" with CQRS and Mediator
- All the messages that return must be translated using the ISharedResource (this is the globalization)
- If a new message needs to be created it must be created inside "HomeControllerHub.Globalization/Resources"
- When you will implement a new entire crud, please take a look on the "Profile" crud it is a good example on how to implement
- If a new DTO must be created for a entity it must follow this:
    - NameOfDto : IMapFrom<EntityThatNeedsTheMapping>
    - If needs the pagination NameOfDto : IMapFrom<NameOfDto>, IPaginatedDto

# Errors
- All errors or exceptions must use the AppError for example:
    - throw new AppError(code of the exception in http, message, description) 
    - The code and message are required but the description is optional

# Entity Creation
- Every entity must be created on "HomeControllerHub.Domain/Entities"
- Every entity must inherit from "Base"
- Every entity must have a configuration file that must be created on "HomeControllerHub.Domain/Entities/Configuration"
- Every entity must be create on singular name not plural, the plural is only on the database 
- Every entity and its configuration must be added on the ApplicationDbContext

# Controllers
- Every entity created will have a complete crud unless it is specified to not have an entire crud
- The controller name will be the same as the entity name
- The routes for the crud will be 
    - POST(nohting more on the name) -> make the entity creation
    - PUT(nohting more on the name) -> make the entity edit
    - DELETE(nohting more on the name) -> make the entity deletion
    - GET(list) -> list all entities at once
    - GET(nohting more on the name) -> list all entities paginated
    - GET({id}) -> list only one entity by id
- The controller will never execute the business rules, it will delegate this task for the commands or query

# Commands and Queries General
- All of the commands and queries must have the Authorzie attribute unless you see that the user does not need the authorization or it is explicity told; not the one that comes from the microsoft "HomeControllerHUB.Shared.Common";

# Commands
- Commands must be created inside the "HomeControllerHUB.Application"
- All of the creation commands must return the id of the created entity
- All the update or delete commands must return 204; (it can be created on a return type "Task")
- A folder must be created "NameOfTheEntity" in the plural and inside it will have the "Commands" and "Queries"
- A subfolder must be created with the name of the action for example "CreateUser" and inside the "CreateUser"
    - CreateUserCommand
    - CreateUserCommandValidator
- For every command created it needs to be create a CommandValidator;
    - The command validator implements an "AbstractValidator<NameOfTheCommand>" and the rules for the creation of that entity must be put there

# Queries
- Queries must be created inside the "HomeControllerHUB.Application"
- All of the queries must return a something
- A folder must be created "NameOfTheEntity" in the plural and inside it will have the "Commands" and "Queries"
- A subfolder must be created with the name of the action for example "GetUser" and inside the "GetUser"
    - GetUserQuery
    - GetUserQueryValidator -> this is not required, you only add this if you need to valid a query parameter for search purposes


# Services
- Services are created inside the "HomeControllerHub.Infra"
- The services must be created when its logic will be used on a lot of places