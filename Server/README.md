# ASP.NET Core Web API with Entity Framework Core

This is a Web API project built using ASP.NET Core and Entity Framework Core (EF Core) with a code-first approach.

## Prerequisites

- [.NET 8 SDK or later](https://dotnet.microsoft.com/download)
- SQL Server
- Optional: Visual Studio and Microsoft SQL Server Managment Studio

## Getting Started

### Configure the Database

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=[servername];Database=[database name];Trusted_Connection=True;User Id=[user id];Password=[password];MultipleActiveResultSets=True;TrustServerCertificate=True;Integrated security=false"
}
```

### Apply Migrations and Create the Database

Install the EF Core CLI if not already installed:

```bash
dotnet tool install --global dotnet-ef
```

Apply migrations:

```bash
dotnet ef database update
```

## Build and Run

### Restore Dependencies

```bash
dotnet restore
```

### Build the Project

```bash
dotnet build
```

### Run the API

```bash
dotnet run
```

## Test the API

- Navigate to Swagger UI: https://localhost:5001/swagger
- Use tools like Postman, curl, or your browser to test endpoints

## Common EF Core CLI Commands

```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
dotnet ef database drop
```
