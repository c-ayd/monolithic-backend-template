## About

This template provides infrastructure to develop a monolithic backend quickly.

## How to Start
In order to start the application, you need to set some secret values that the application uses. The general strucure of the secrests (without any values) can be found in `appsettings.json`. For development, you can use user secrets to configure these secrets without revealing the values in your repository. In general, it is also highly recommended not to put any secrets in your code.

There are also some environment variables that are needed to configure the database and the certificate for HTTPS. The expected variable names are as follows and they can also be found and changed in the `docker-compose.yaml` file.
```
POSTGRES_USER=...
POSTGRES_PASSWORD=...
POSTGRES_DB=...

CERT_PATH=...
CERT_PASSWORD=...
```

Afterwards, you can search for the keyword `NOTE:` in the entire project to find the customizable and required parts of the code.

After setting up everything, you can either run the backend locally or use the docker compose in the project. To run the application on Docker, you can use the following command while you are on the root path of the project:
```
docker compose up --build -d
```

## Structure
The project uses the onion architecture to separate the responsibilities of the entire application for maintability. It also uses the vertical slice architecure along with the CQRS pattern to develop each part of the business logic independently (or contanirize each part into one class). So that, the code can be easily maintained and developed fast. Otherwise, other approaches, such as services, might end up being too big to maintain due to the nature of the monolithic architecture.

Furthermore, the project utilizes `Entity Framework Core` (an ORM library) along with the repository and unit of work patterns. This design is chosen to decouple EF Core from the business logic. So that, when you need to switch the ORM library or remove it entirely, no change in the application layer is needed.

In addition, `Cayd.AspNetCore.FlexLog` is used to log HTTP requests and is configured to use a database as the primary storage and a file as the backup. This approach is only for demonstration purposes. You may want to consider saving logs to different files based on time (for example, daily or montly) or may choose to remove the existing library and replace it by another logging library.

## Key Concepts
Being familiar with the following concepts can help you to understand the code better:
- Monolithic architecture
- Onion architecture
- Vertical slice architecture
- CQRS pattern (along with the mediator pattern)
- Repository pattern
- Unit of work pattern
- Entity Framework Core (or ORM in general)
- JWT bearer authentication
- Policy-based authorization
- Hashing and encryption
- SMTP
- MVC
- Localization
- CORS
- Rate limiter
- ASP.NET Core data protection