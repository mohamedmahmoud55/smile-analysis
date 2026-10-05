# SmileAnalysis

An ASP.NET Core MVC dental-clinic application built with .NET 10, Entity Framework Core, and SQL Server.

## Run locally

Requirements:

- .NET 10 SDK
- SQL Server

Set `ConnectionStrings__DefaultConnection` to a SQL Server connection string, then run:

```powershell
dotnet run --project .\SmileAnalysisPl\SmileAnalysisPl.csproj
```

On first startup, EF Core applies database migrations. To create the first administrator in a new database, set these environment variables before starting the app:

- `InitialAdmin__Email`
- `InitialAdmin__Password` (use a unique, strong password)
- Optionally, `InitialAdmin__FirstName`, `InitialAdmin__LastName`, and `InitialAdmin__PhoneNumber`

The app does not contain a default administrator password. If the initial-admin variables are absent, the roles are created but no user is seeded.

## Free public demo hosting

[Somee's free ASP.NET hosting](https://www.somee.com/FreeAspNetHosting.aspx) advertises ASP.NET Core 10 and SQL Server Express without requiring payment details. Its published limits include forced advertising, 150 MB website storage, 5 GB monthly transfer, and one 30 MB SQL database. Inactive sites or databases may be removed. Check the current terms before signing up.

This is a demo/portfolio deployment, not a production clinical system. Do not enter real patient data or upload real clinical images.

1. Create a free ASP.NET Core website and SQL Server database with Somee (or another host that supports .NET 10).
2. Publish the web project:

   ```powershell
   dotnet publish .\SmileAnalysisPl\SmileAnalysisPl.csproj --configuration Release --output .\publish
   ```

3. Upload the contents of `publish` to the website's application root using the host's supported deployment method.
4. Configure these settings in the hosting control panel or on the host (never commit credentials to GitHub):
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection` with the host-provided SQL Server connection string
   - `InitialAdmin__Email`
   - `InitialAdmin__Password` with a unique, strong password
5. Open the website URL. Startup applies the EF migrations and creates the initial administrator if the database has no users. After the first successful startup, remove the initial-admin password setting from the host configuration.

The free database and hosting limits make this unsuitable for real clinic operations or patient records.

## Public-repository exclusions

The repository ignores local uploads, member profile images, build output, and IDE metadata. Keep real patient records, clinical images, connection strings, and passwords out of the repository.
