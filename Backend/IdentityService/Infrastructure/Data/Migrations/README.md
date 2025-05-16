# Database Migration Instructions

To create the initial database migration, run the following command from the solution directory:

```bash
# Make sure you have the Entity Framework Core tools installed
dotnet tool install --global dotnet-ef

# Create the migration
cd IdentityService/API
dotnet ef migrations add InitialCreate --project ../Infrastructure/IdentityService.Infrastructure.csproj --startup-project ./IdentityService.API.csproj

# Apply the migration to the database
dotnet ef database update --project ../Infrastructure/IdentityService.Infrastructure.csproj --startup-project ./IdentityService.API.csproj
```

## Initial Data

The database will be automatically seeded with the following data:

### Roles
- Admin
- User
- Manager

### Users
1. Admin User
   - Username: admin
   - Email: admin@batteryshop.com
   - Password: Admin@123
   - Roles: Admin, User

2. Regular User
   - Username: user
   - Email: user@batteryshop.com
   - Password: User@123
   - Roles: User

## API Endpoints for Database Management

The following API endpoints are available for managing the database:

- `POST /api/Database/seed` - Seeds the database (requires Admin role)
- `POST /api/Database/seed/anonymous?apiKey=development_seed_key_123` - Seeds the database without authentication (development only)

These endpoints are useful for testing and re-initializing the database if needed.
