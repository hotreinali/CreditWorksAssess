namespace CreditWorks.Api.Features.Vehicles;

public sealed record CreateVehicleRequest(
    string? OwnerName,
    int ManufacturerId,
    int YearOfManufacture,
    decimal WeightKg);

public sealed record ManufacturerResponse(int Id, string Name);

public sealed record CategorySummary(int Id, string Name, string IconKey);

public sealed record VehicleResponse(
    int Id,
    string OwnerName,
    ManufacturerResponse Manufacturer,
    int YearOfManufacture,
    decimal WeightKg,
    CategorySummary Category);

public enum VehicleSortField
{
    OwnerName,
    Manufacturer,
    YearOfManufacture,
    Weight
}

public enum SortDirection
{
    Ascending,
    Descending
}
