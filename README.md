# ArtiX

A multi-layered .NET 8 solution with Domain, Application, Infrastructure, and API projects.

## Prerequisites
- .NET 8 SDK installed locally
- Access to `nuget.org` package source
- SQL Server instance available (default connection targets `localhost\\SQLEXPRESS` with the `sa` login)

## Restore and build
Run the following from the solution root:

```bash
# Restore NuGet packages
 dotnet restore ArtiX.sln

# Build the solution
 dotnet build ArtiX.sln
```

## Apply database migrations
Ensure the SQL Server connection string in `ArtiX.Api/appsettings.json` is correct, then run:

```bash
 dotnet ef database update --project ArtiX.Infrastructure --startup-project ArtiX.Api
```

## Notes
The current environment may not include the .NET SDK, so the commands above must be executed where the SDK is available.
