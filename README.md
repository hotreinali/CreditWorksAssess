# CreditWorks vehicle management

An ASP.NET Core 10 API and React application for managing vehicles and weight categories.

## Development

Requirements: .NET 10 SDK, Node.js, npm, and SQL Server 2019 or later (database setup is added in the next milestone).

Start the API with `dotnet run --project backend/CreditWorks.Api` and the frontend with `cd frontend && npm install && npm run dev`. Run tests with `dotnet test CreditWorksAssess.slnx`.

The API health endpoint is `/api/health`. Further setup and design notes will be added as the application is built.
