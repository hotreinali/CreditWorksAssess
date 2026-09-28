# CreditWorks Vehicle Management

A small production-style application for registering vehicles and maintaining the weight categories used to classify them. The backend uses ASP.NET Core 10, Entity Framework Core and SQL Server 2019 or later. The user interface uses React and TypeScript.

## Features

- Add vehicles with server-side and browser validation.
- View and sort vehicles by owner, manufacturer, year or weight in either direction.
- Calculate every vehicle's category from its weight and the current category configuration.
- Add, edit and delete weight categories as one validated configuration.
- Prevent category gaps, overlaps and stale concurrent updates.
- Show category icons and responsive feedback throughout the React interface.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Node.js 22.12 or later and npm
- Microsoft SQL Server 2019 or later
- EF Core CLI 10.0.12 for database migrations
- Docker Desktop only if using the optional containerised SQL Server setup

Install the EF Core CLI once if it is not already available:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
```

## Database setup

The application only depends on a SQL Server connection string. Docker is optional.

### Option A: existing SQL Server

Create or use a SQL Server login that can create the development database, then set the connection string in the terminal that will run EF Core and the API:

```bash
export ConnectionStrings__CreditWorks='Server=localhost,1433;Database=CreditWorks;User Id=sa;Password=<your password>;TrustServerCertificate=true'
```

Replace the server and credentials with those for your SQL Server instance. On Windows, an integrated-security connection string can be used instead.

### Option B: Docker SQL Server

The included [compose.yaml](compose.yaml) starts SQL Server 2022 Developer Edition and stores its data in a named volume. Set a local password that meets SQL Server's complexity requirements, then start the container:

```bash
export MSSQL_SA_PASSWORD='choose-a-strong-local-password'
docker compose up -d
```

Set the API connection string with the same password:

```bash
export ConnectionStrings__CreditWorks="Server=localhost,1433;Database=CreditWorks;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=true"
```

Wait until `docker compose logs sqlserver` reports that SQL Server is ready. The image runs as `linux/amd64`, allowing Apple Silicon Docker Desktop to use emulation. To stop it without deleting data, run `docker compose stop`.

### Create the schema

From the repository root, with `ConnectionStrings__CreditWorks` set:

```bash
dotnet ef database update \
  --project backend/CreditWorks.Api \
  --startup-project backend/CreditWorks.Api
```

The migration creates all tables and seeds Mazda, Mercedes, Honda, Ferrari and Toyota, plus the initial Light, Medium and Heavy categories. No manual SQL scripts are required.

## Run the application

Restore and start the API from the repository root:

```bash
dotnet restore CreditWorksAssess.slnx
dotnet run --project backend/CreditWorks.Api
```

The API listens at `http://localhost:5000`. Verify it with:

```bash
curl http://localhost:5000/api/health
```

In a second terminal, start React:

```bash
cd frontend
npm install
npm run dev
```

Open [http://localhost:5173](http://localhost:5173). Vite proxies `/api` requests to the API at port 5000 during development.

For a production frontend build:

```bash
cd frontend
npm run build
```

The output is written to `frontend/dist`. A production deployment should serve this directory and proxy `/api` to the ASP.NET Core application on the same origin. Alternatively, set `VITE_API_BASE_URL` before building and configure the hosting environment accordingly.

## Tests and checks

Run the backend business-rule and relational integration tests:

```bash
dotnet test CreditWorksAssess.slnx
```

Run the frontend static checks and production build:

```bash
cd frontend
npm run lint
npm run build
```

The tests concentrate on category boundaries, invalid configurations, transaction behaviour, stale versions, dynamic reclassification, vehicle validation and every supported sort field.

## Architecture and data design

The solution uses a feature-oriented structure without additional layers that would add little value at this size:

- Minimal API endpoint classes handle HTTP binding and responses.
- Feature services coordinate validation, EF Core queries and transactions.
- Domain helpers contain category rules that can be tested without HTTP or SQL Server.
- `AppDbContext` owns persistence mapping, database constraints and seed data.
- React components separate API access, vehicle entry, the fleet list and category administration.

The database contains `Vehicles`, `Manufacturers`, `Categories` and a single-row `CategoryConfigurations` version record. Manufacturers are rows rather than source-code enums, allowing the list to change without altering vehicle records. Vehicle weight and category boundaries use `decimal(18,2)`.

A vehicle does not store a category ID. Its category is calculated when vehicles are read, using the latest category boundaries. This avoids stale category assignments after an administrator changes the configuration.

Category updates replace the complete proposed configuration inside a database transaction. The configuration version is an optimistic-concurrency token: every successful save increments it, while a request using an old version receives HTTP `409 Conflict`.

## Category boundary rules

Valid vehicle weight starts at `0.01 kg`. Each category stores one inclusive lower boundary; its upper boundary is the next category's lower boundary and is exclusive. The last category has no upper limit.

Initial ranges are therefore:

| Category | Range |
| --- | --- |
| Light | `0.01 ≤ weight < 500.00` |
| Medium | `500.00 ≤ weight < 2500.00` |
| Heavy | `weight ≥ 2500.00` |

Exactly `500.00 kg` is Medium and exactly `2500.00 kg` is Heavy. The first boundary must be `0.01`, and all following boundaries must be unique positive values with at most two decimal places. Because an upper boundary is derived from the next lower boundary, gaps and overlaps cannot be represented.

## API summary

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/health` | Basic API availability check |
| `GET` | `/api/manufacturers` | List manufacturers |
| `POST` | `/api/vehicles` | Validate and create a vehicle |
| `GET` | `/api/vehicles?sortBy=ownerName&direction=asc` | List and sort dynamically classified vehicles |
| `GET` | `/api/categories` | Read the complete category configuration and version |
| `PUT` | `/api/categories` | Validate and atomically replace the complete configuration |
| `GET` | `/api/category-icons` | List accepted category icon keys |

Vehicle `sortBy` values are `ownerName`, `manufacturer`, `yearOfManufacture` and `weight`. Direction is `asc` or `desc`. Invalid requests use RFC 7807 validation responses. Expected concurrency conflicts return HTTP 409, and unhandled failures return generic error details without exposing stack traces.

## Assumptions and limitations

- A manufacture year from 1886 through the next calendar year is considered sensible, allowing newly announced model-year vehicles.
- Category icons come from a controlled built-in set rather than uploaded files.
- Authentication and authorisation are intentionally omitted because the assignment does not require them.
- The vehicle list is not paginated; paging and server-side filtering should be added before supporting large datasets.
- The application expects one category administrator at a time but safely detects overlapping edits through version conflicts.
- A production system should add authenticated administration, audit history, observability, backup procedures and deployment-specific secret management.
- Automated relational tests use an isolated SQLite database for transaction behaviour. SQL Server-specific schema creation is supplied by the EF Core migration and should also be exercised in the target deployment environment.

## Repository contents

```text
backend/CreditWorks.Api     ASP.NET Core API, domain rules, EF Core model and migrations
backend/CreditWorks.Tests   Unit and relational integration tests
frontend                    React and TypeScript user interface
compose.yaml                Optional local SQL Server 2022 environment
```

Build output, database files, passwords, API keys and production connection strings are intentionally excluded from source control.
