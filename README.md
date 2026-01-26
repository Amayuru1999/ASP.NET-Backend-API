# Backend API

Web API fetches from public JSONPlaceholder API and caches them in the SQL server using no ORM with ASP.NET Core.

## Tech Stack 

* .NET 9
* SQL Server
* Microsoft.Data.SqlClient

## Library Explanation

* Microsoft.Data.SqlClient - ADO.NET(Microsoft core data access technology) direct access to the SQL server with parameterized commands without need of an ORM 
* HttpClientFactory - From this library it is creating centralized and reusable HTTP client creation with a well defined connection pooling and able to adapt for calling external API
* Microsoft.AspNetCore.OpenApi - From this it is generating the Swagger/OpenAPI metadata, from that we can discover and test the API very easily.

## External API
JSON Placeholder posts: https://jsonplaceholder.typicode.com/

## Database Schema

Backend.Api/database/schema.sql contains the schema and it creates:
* Database: PostDb
* Table: dbo.Posts(Id, UserId, Title, Body, FetchedAtUtc)

## Caching Mechanism
* GET /api/posts : JSONPlaceholder API fetches all posts if the database is empty. Otherwise it is returning the cached records.
* GET /api/posts/{id}: If the post is not in the database is fetching data from the JSONPlaceholder.

## Endpoints
* GET /api/posts
* GET /api/posts/{id}

## Prerequesties
* .NET SDK 9.0
* SQL Server(local or Docker - In this project I have used a Docker instance)

## Configure Connection String 
* In appsettings.json you must configure the database connection string and the external apis urls


```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PostDb;User ID=sa;Password=;TrustServerCertificate=True;"
  },
  "ExternalApi": {
    "BaseUrl": "https://jsonplaceholder.typicode.com"
  }
}
```
 
## Export env in the terminal
### macOS / Linux (bash/zsh)
```bash
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=PostDb;User ID=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True;"
```

### Windows PowerShell
```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=PostDb;User ID=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True;"
```

### Windows Command Prompt (cmd)
```bat
set ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=PostDb;User ID=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True;
```

## Create SQL Server container
### macOS / Linux (bash/zsh)
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YOUR_PASSWORD>" \
  -p 1433:1433 --name my-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

### Windows PowerShell
```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YOUR_PASSWORD>" `
  -p 1433:1433 --name my-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

### Windows Command Prompt (cmd)
```bat
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YOUR_PASSWORD>" ^
  -p 1433:1433 --name my-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

## Copy and Apply Schema
### macOS / Linux (bash/zsh)
```bash
docker cp Backend.Api/database/schema.sql my-sql:/var/opt/mssql/data/schema.sql
docker exec -i my-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 -U sa -P "<YOUR_PASSWORD>" -C \
  -i /var/opt/mssql/data/schema.sql
```

### Windows PowerShell
```powershell
docker cp Backend.Api\database\schema.sql my-sql:/var/opt/mssql/data/schema.sql
docker exec -i my-sql /opt/mssql-tools18/bin/sqlcmd `
  -S localhost,1433 -U sa -P "<YOUR_PASSWORD>" -C `
  -i /var/opt/mssql/data/schema.sql
```

### Windows Command Prompt (cmd)
```bat
docker cp Backend.Api\database\schema.sql my-sql:/var/opt/mssql/data/schema.sql
docker exec -i my-sql /opt/mssql-tools18/bin/sqlcmd ^
  -S localhost,1433 -U sa -P "<YOUR_PASSWORD>" -C ^
  -i /var/opt/mssql/data/schema.sql
```
## Build and Run
```bash
cd Backend.Api

dotnet restore

dotnet build

dotnet run
```

## Example Requests

```bash
curl http://localhost:5000/api/posts
```
```bash
curl http://localhost:5000/api/posts/1
```
