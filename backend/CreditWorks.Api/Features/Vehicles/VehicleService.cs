using CreditWorks.Api.Data;
using CreditWorks.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Api.Features.Vehicles;

public sealed class VehicleService(AppDbContext dbContext, TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<ManufacturerResponse>> GetManufacturersAsync(CancellationToken cancellationToken) =>
        await dbContext.Manufacturers.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ManufacturerResponse(x.Id, x.Name))
            .ToListAsync(cancellationToken);

    public async Task<VehicleCreationResult> CreateAsync(
        CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var errors = VehicleValidation.Validate(request, timeProvider.GetUtcNow().Year);
        if (errors.Count != 0)
            return VehicleCreationResult.Invalid(errors);

        var manufacturer = await dbContext.Manufacturers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.ManufacturerId, cancellationToken);
        if (manufacturer is null)
            return VehicleCreationResult.Invalid(new Dictionary<string, string[]>
            {
                [nameof(request.ManufacturerId)] = ["The selected manufacturer does not exist."]
            });

        var categories = await dbContext.Categories.AsNoTracking()
            .OrderBy(x => x.StartsAtKg)
            .ToListAsync(cancellationToken);
        if (CategoryRanges.Validate(categories).Count != 0)
            throw new InvalidOperationException("Vehicle categories are not configured correctly.");

        var vehicle = new Vehicle
        {
            OwnerName = request.OwnerName!.Trim(),
            ManufacturerId = manufacturer.Id,
            YearOfManufacture = request.YearOfManufacture,
            WeightKg = request.WeightKg
        };
        dbContext.Vehicles.Add(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);

        return VehicleCreationResult.Success(Map(vehicle, manufacturer, CategoryRanges.Find(vehicle.WeightKg, categories)));
    }

    public async Task<IReadOnlyList<VehicleResponse>> GetVehiclesAsync(
        VehicleSortField sortField,
        SortDirection direction,
        CancellationToken cancellationToken)
    {
        IQueryable<Vehicle> query = dbContext.Vehicles.AsNoTracking().Include(x => x.Manufacturer);
        query = (sortField, direction) switch
        {
            (VehicleSortField.OwnerName, SortDirection.Ascending) => query.OrderBy(x => x.OwnerName).ThenBy(x => x.Id),
            (VehicleSortField.OwnerName, SortDirection.Descending) => query.OrderByDescending(x => x.OwnerName).ThenBy(x => x.Id),
            (VehicleSortField.Manufacturer, SortDirection.Ascending) => query.OrderBy(x => x.Manufacturer.Name).ThenBy(x => x.Id),
            (VehicleSortField.Manufacturer, SortDirection.Descending) => query.OrderByDescending(x => x.Manufacturer.Name).ThenBy(x => x.Id),
            (VehicleSortField.YearOfManufacture, SortDirection.Ascending) => query.OrderBy(x => x.YearOfManufacture).ThenBy(x => x.Id),
            (VehicleSortField.YearOfManufacture, SortDirection.Descending) => query.OrderByDescending(x => x.YearOfManufacture).ThenBy(x => x.Id),
            (VehicleSortField.Weight, SortDirection.Ascending) => query.OrderBy(x => x.WeightKg).ThenBy(x => x.Id),
            (VehicleSortField.Weight, SortDirection.Descending) => query.OrderByDescending(x => x.WeightKg).ThenBy(x => x.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(sortField))
        };

        var vehicles = await query.ToListAsync(cancellationToken);
        var categories = await dbContext.Categories.AsNoTracking()
            .OrderBy(x => x.StartsAtKg)
            .ToListAsync(cancellationToken);
        if (CategoryRanges.Validate(categories).Count != 0)
            throw new InvalidOperationException("Vehicle categories are not configured correctly.");

        return vehicles.Select(vehicle =>
            Map(vehicle, vehicle.Manufacturer, CategoryRanges.Find(vehicle.WeightKg, categories))).ToArray();
    }

    private static VehicleResponse Map(Vehicle vehicle, Manufacturer manufacturer, Category category) =>
        new(
            vehicle.Id,
            vehicle.OwnerName,
            new ManufacturerResponse(manufacturer.Id, manufacturer.Name),
            vehicle.YearOfManufacture,
            vehicle.WeightKg,
            new CategorySummary(category.Id, category.Name, category.IconKey));
}

public sealed record VehicleCreationResult(VehicleResponse? Vehicle, Dictionary<string, string[]> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static VehicleCreationResult Success(VehicleResponse vehicle) => new(vehicle, []);

    public static VehicleCreationResult Invalid(Dictionary<string, string[]> errors) => new(null, errors);
}
