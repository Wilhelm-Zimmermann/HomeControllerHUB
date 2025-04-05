# General Informations
- All the messages that return must be translated using the ISharedResource (this is the globalization)
- If a new message needs to be created it must be created inside "HomeControllerHub.Globalization/Resources"

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
