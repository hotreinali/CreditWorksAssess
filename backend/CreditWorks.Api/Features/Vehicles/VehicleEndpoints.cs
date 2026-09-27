namespace CreditWorks.Api.Features.Vehicles;

public static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/manufacturers", async (
            VehicleService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetManufacturersAsync(cancellationToken)));

        endpoints.MapGet("/api/vehicles", async (
            string? sortBy,
            string? direction,
            VehicleService service,
            CancellationToken cancellationToken) =>
        {
            if (!TryParseSortField(sortBy, out var sortField))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["sortBy"] = ["Sort by ownerName, manufacturer, yearOfManufacture, or weight."]
                });
            if (!TryParseDirection(direction, out var sortDirection))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["direction"] = ["Direction must be asc or desc."]
                });

            return Results.Ok(await service.GetVehiclesAsync(sortField, sortDirection, cancellationToken));
        });

        endpoints.MapPost("/api/vehicles", async (
            CreateVehicleRequest request,
            VehicleService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CreateAsync(request, cancellationToken);
            return result.IsValid
                ? Results.Created("/api/vehicles", result.Vehicle)
                : Results.ValidationProblem(result.Errors);
        });

        return endpoints;
    }

    private static bool TryParseSortField(string? value, out VehicleSortField sortField)
    {
        sortField = value?.ToLowerInvariant() switch
        {
            null or "ownername" => VehicleSortField.OwnerName,
            "manufacturer" => VehicleSortField.Manufacturer,
            "yearofmanufacture" => VehicleSortField.YearOfManufacture,
            "weight" => VehicleSortField.Weight,
            _ => (VehicleSortField)(-1)
        };
        return Enum.IsDefined(sortField);
    }

    private static bool TryParseDirection(string? value, out SortDirection direction)
    {
        direction = value?.ToLowerInvariant() switch
        {
            null or "asc" => SortDirection.Ascending,
            "desc" => SortDirection.Descending,
            _ => (SortDirection)(-1)
        };
        return Enum.IsDefined(direction);
    }
}
