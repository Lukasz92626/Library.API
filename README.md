# Library.API

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED.svg)](https://www.docker.com/)

REST API for library management.

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

- **API:** http://localhost:5055
- **Swagger:** http://localhost:5055/swagger
- **PostgreSQL:** localhost:5432