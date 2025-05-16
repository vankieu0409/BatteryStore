# IdentityService Database Management

This document explains how the database initialization and data seeding works in the IdentityService.

## Automatic Database Initialization

The application automatically initializes the database and seeds initial data when it starts. This is done through the following process:

1. The `InitializeDatabaseAsync` extension method is called in `Program.cs`
2. This method:
   - Creates the database if it doesn't exist
   - Applies any pending migrations
   - Seeds initial data if the database is empty

## Database Seed Data

The initial seed data includes:

### Roles
- Admin - Full access to the system
- User - Basic user access
- Manager - Management access

### Default Users
- Admin user:
  - Username: admin
  - Email: admin@batteryshop.com
  - Password: Admin@123
  - Roles: Admin, User

- Regular user:
  - Username: user
  - Email: user@batteryshop.com
  - Password: User@123
  - Roles: User

## Manual Database Seeding

You can also trigger database seeding manually through API endpoints:

- `POST /api/Database/seed` - Requires authentication with Admin role
- `POST /api/Database/seed/anonymous?apiKey=development_seed_key_123` - No authentication required, but needs API key

## Connection String

The database connection string is configured in `appsettings.json` and `appsettings.Development.json` under the key `ConnectionStrings:IdentityDb`.

Example:
```json
"ConnectionStrings": {
  "IdentityDb": "Host=localhost;Port=5432;Database=batteryshop_identity;Username=postgres;Password=postgres"
}
```

## Migrations

To create new migrations after modifying entity models:

```bash
dotnet ef migrations add <MigrationName> --project ../Infrastructure/IdentityService.Infrastructure.csproj --startup-project ./IdentityService.API.csproj
```

To apply migrations to the database:

```bash
dotnet ef database update --project ../Infrastructure/IdentityService.Infrastructure.csproj --startup-project ./IdentityService.API.csproj
```
