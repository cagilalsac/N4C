# N4C

- Core Framework for web applications and restful services developed with .NET.

# Features:
- N-Layered Architecture: Controller <-> Model (Request, Response) <-> Service <-> Entity <-> DbContext.
- Clean Architecture with CQRS: Controller <-> Request, Response <-> Mediator <-> Handler <-> Service <-> Entity <-> DbContext.
- Generic Service CRUD operations.
- Generic Handlers using CRUD operation Service methods.
- Built-in AutoMapper implementation with customized mapping configuration.
- Automatic trimming string properties of objects.
- Automatic update of Deleted, CreateDate, CreatedBy, UpdateDate and UpdatedBy properties of entities if they exist.
- Multiple files upload with file download.
- Displaying a money decimal value in text format.
- Checking intersections in a list of date time for start and end date time values.
- Finding an expression in a string value by case or word matching.
- Paging and ordering lists.
- Excel export.
- English and Turkish languages support.
- MVC Account Management with users and roles.
- MVC Authentication using cookie and API Authentication using JWT.
- Customized scaffolding for API and MVC controllers with MVC views.