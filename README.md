# CreditWorks vehicle management

An ASP.NET Core 10 API and React application for managing vehicles and weight categories.

## Development

Requirements: .NET 10 SDK, Node.js, npm, and SQL Server 2019 or later.

Start the API with `dotnet run --project backend/CreditWorks.Api` and the frontend with `cd frontend && npm install && npm run dev`. Run tests with `dotnet test CreditWorksAssess.slnx`.

Set the `ConnectionStrings__CreditWorks` environment variable to a connection string for your SQL Server database. For example, use `Server=localhost,1433;Database=CreditWorks;User Id=sa;Password=<your password>;TrustServerCertificate=true` with your own credentials. Do not commit credentials.

Apply the initial migration with `dotnet ef database update --project backend/CreditWorks.Api --startup-project backend/CreditWorks.Api` after installing `dotnet-ef` version 10.0.12. The migration creates the schema and seeds five manufacturers and the initial Light, Medium and Heavy categories.

The API health endpoint is `/api/health`. Further feature setup and design notes will be added as the application is built.

## Vehicle API

With the API running at `http://localhost:5000`, the available vehicle endpoints are:

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/manufacturers` | List the predefined manufacturers |
| `POST` | `/api/vehicles` | Validate and create a vehicle |
| `GET` | `/api/vehicles?sortBy=ownerName&direction=asc` | List vehicles with their current categories |
| `GET` | `/api/categories` | Read the complete category configuration and version |
| `PUT` | `/api/categories` | Atomically replace the complete category configuration |
| `GET` | `/api/category-icons` | List the icon keys accepted for categories |

`sortBy` accepts `ownerName`, `manufacturer`, `yearOfManufacture`, or `weight`. `direction` accepts `asc` or `desc`; both parameters default to owner name ascending.

Example vehicle request:

```json
{
  "ownerName": "Jane Turei",
  "manufacturerId": 2,
  "yearOfManufacture": 2020,
  "weightKg": 1850.75
}
```

Vehicle years must be from 1886 through the next calendar year. Invalid input returns an RFC 7807 validation response without writing a vehicle.

Category changes are submitted as one complete configuration. The request must include the version returned by `GET /api/categories`; a stale version receives HTTP `409 Conflict` and must reload before retrying. Omitting a category deletes it, adding an item creates it, and changing an item modifies its definition. The API validates the entire proposed configuration before applying it in a transaction.

## Category boundaries

Vehicle weights use kilograms with two decimal places and start at 0.01 kg. Each category stores its inclusive lower boundary. Its upper boundary is the next category's lower boundary and is exclusive; the final category has no upper boundary. Initial boundaries are 0.01, 500.00 and 2500.00 kg, so exactly 500.00 kg is Medium and exactly 2500.00 kg is Heavy. This model has no representable gaps or overlaps when the first boundary is 0.01 and all boundaries are unique.
