using CreditWorks.Api.Data;
using CreditWorks.Api.Features.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Tests;

public sealed class VehicleServiceTests
{
    [Fact]
    public async Task Create_trims_owner_and_returns_current_category()
    {
        await using var db = CreateDatabase();
        var service = new VehicleService(db, TimeProvider.System);

        var result = await service.CreateAsync(
            new CreateVehicleRequest("  Jane Turei  ", 2, 2020, 1850.75m),
            CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal("Jane Turei", result.Vehicle!.OwnerName);
        Assert.Equal("Mercedes", result.Vehicle.Manufacturer.Name);
        Assert.Equal("Medium", result.Vehicle.Category.Name);
        Assert.Equal(1850.75m, (await db.Vehicles.SingleAsync()).WeightKg);
    }

    [Fact]
    public async Task Create_rejects_unknown_manufacturer_without_saving()
    {
        await using var db = CreateDatabase();
        var service = new VehicleService(db, TimeProvider.System);

        var result = await service.CreateAsync(
            new CreateVehicleRequest("Jane Turei", 999, 2020, 1200m),
            CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains("ManufacturerId", result.Errors.Keys);
        Assert.Empty(await db.Vehicles.ToListAsync());
    }

    [Fact]
    public async Task Lists_manufacturers_alphabetically()
    {
        await using var db = CreateDatabase();
        var service = new VehicleService(db, TimeProvider.System);

        var manufacturers = await service.GetManufacturersAsync(CancellationToken.None);

        Assert.Equal(["Ferrari", "Honda", "Mazda", "Mercedes", "Toyota"], manufacturers.Select(x => x.Name));
    }

    [Fact]
    public async Task Supports_all_vehicle_sort_options()
    {
        await using var db = CreateDatabase();
        db.Vehicles.AddRange(
            new Vehicle { OwnerName = "Zoe", ManufacturerId = 1, YearOfManufacture = 2019, WeightKg = 600m },
            new Vehicle { OwnerName = "Alex", ManufacturerId = 2, YearOfManufacture = 2022, WeightKg = 2800m },
            new Vehicle { OwnerName = "Mia", ManufacturerId = 3, YearOfManufacture = 2015, WeightKg = 400m });
        await db.SaveChangesAsync(CancellationToken.None);
        var service = new VehicleService(db, TimeProvider.System);

        var byOwner = await service.GetVehiclesAsync(VehicleSortField.OwnerName, SortDirection.Ascending, CancellationToken.None);
        var byManufacturer = await service.GetVehiclesAsync(VehicleSortField.Manufacturer, SortDirection.Descending, CancellationToken.None);
        var byYear = await service.GetVehiclesAsync(VehicleSortField.YearOfManufacture, SortDirection.Ascending, CancellationToken.None);
        var byWeight = await service.GetVehiclesAsync(VehicleSortField.Weight, SortDirection.Descending, CancellationToken.None);

        Assert.Equal(["Alex", "Mia", "Zoe"], byOwner.Select(x => x.OwnerName));
        Assert.Equal(["Alex", "Zoe", "Mia"], byManufacturer.Select(x => x.OwnerName));
        Assert.Equal(["Mia", "Zoe", "Alex"], byYear.Select(x => x.OwnerName));
        Assert.Equal(["Alex", "Zoe", "Mia"], byWeight.Select(x => x.OwnerName));
        Assert.Equal(["Heavy", "Medium", "Light"], byWeight.Select(x => x.Category.Name));
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
