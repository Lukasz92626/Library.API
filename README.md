# Library.API

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED.svg)](https://www.docker.com/)

REST API for library management. Library.API is a RESTful web service for managing library resources. It follows a clean architecture approach, separating concerns across Application, Domain, Infrastructure, and API layers.

## Project Structure

    Library.API/
    ├── .config/                  # Configuration files
    ├── Library.API/              # API layer (controllers, middleware, DI)
    ├── LIbrary.Application/      # Application layer (use cases, business logic)
    ├── Library.Domain/           # Domain layer (entities, core models)
    ├── Library.Infrastructure/   # Infrastructure layer (EF Core, DB context, migrations)
    ├── Library.Tests/            # Unit and integration tests
    ├── docker-compose.yaml       # Docker Compose configuration
    ├── Library.sln               # Solution file
    └── README.md

## Quick start with Docker

### Run everything
```bash
docker-compose up --build -d
```

### Check status
```bash
docker-compose ps
```

### View logs
```bash
docker-compose logs -f api
```

### Stop
```bash
docker-compose down
```

## Endpoints

| Service    | URL                              |
|------------|----------------------------------|
| API        | http://localhost:5055            |
| Swagger UI | http://localhost:5055/swagger    |
| PostgreSQL | localhost:5432                   |