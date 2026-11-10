# Brunva server

Phase 1 is identity, organizations, and tenant isolation.

## Run

```bash
docker compose up -d
dotnet run --project src/BrunvaServer.Api
```

The API listens on `http://localhost:5086`. OpenAPI is at `/openapi/v1.json`. Health checks are `/health/live` and `/health/ready`.

Copy `.env.example` when you override the development connection string or signing key. `Jwt:SigningKey` must be at least 32 bytes. Do not commit real secrets.

## Test

```bash
dotnet test BrunvaServer.slnx
```

Integration tests start PostgreSQL with Testcontainers. Docker has to be running.
