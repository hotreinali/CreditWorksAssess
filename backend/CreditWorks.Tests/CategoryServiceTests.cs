using CreditWorks.Api.Data;
using CreditWorks.Api.Features.Categories;
using CreditWorks.Api.Features.Vehicles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Tests;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task Get_returns_version_ordered_ranges_and_open_final_range()
    {
        await using var database = await TestDatabase.CreateAsync();
        var result = await new CategoryService(database.Context).GetAsync(CancellationToken.None);

        Assert.Equal(1, result.Version);
        Assert.Equal(["Light", "Medium", "Heavy"], result.Categories.Select(x => x.Name));
        Assert.Equal(500m, result.Categories[0].EndsBeforeKg);
        Assert.Equal(2500m, result.Categories[1].EndsBeforeKg);
        Assert.Null(result.Categories[2].EndsBeforeKg);
    }

    [Fact]
    public async Task Update_changes_existing_vehicle_category_and_increments_version()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Vehicles.Add(new Vehicle
        {
            OwnerName = "Jane Turei",
            ManufacturerId = 2,
            YearOfManufacture = 2020,
            WeightKg = 2200m
        });
        await database.Context.SaveChangesAsync();
        var categoryService = new CategoryService(database.Context);

        var update = await categoryService.UpdateAsync(new UpdateCategoryConfigurationRequest(1,
        [
            new("Light", "light", 0.01m),
            new("Medium", "medium", 500m),
            new("Heavy", "heavy", 2000m)
        ]), CancellationToken.None);
        var vehicles = await new VehicleService(database.Context, TimeProvider.System)
            .GetVehiclesAsync(VehicleSortField.OwnerName, SortDirection.Ascending, CancellationToken.None);

        Assert.Equal(CategoryUpdateStatus.Success, update.Status);
        Assert.Equal(2, update.Configuration!.Version);
        Assert.Equal("Heavy", Assert.Single(vehicles).Category.Name);
    }

    [Fact]
    public async Task Update_can_add_and_delete_categories_as_one_configuration()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new CategoryService(database.Context);

        var added = await service.UpdateAsync(new UpdateCategoryConfigurationRequest(1,
        [
            new("Micro", "car", 0.01m),
            new("Light", "light", 300m),
            new("Medium", "medium", 1000m),
            new("Heavy", "truck", 3000m)
        ]), CancellationToken.None);
        var deleted = await service.UpdateAsync(new UpdateCategoryConfigurationRequest(2,
        [
            new("Light", "light", 0.01m),
            new("Heavy", "truck", 1000m)
        ]), CancellationToken.None);

        Assert.Equal(4, added.Configuration!.Categories.Count);
        Assert.Equal(2, deleted.Configuration!.Categories.Count);
        Assert.Equal(3, deleted.Configuration.Version);
    }

    [Fact]
    public async Task Invalid_configuration_is_rejected_without_database_changes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new CategoryService(database.Context);

        var result = await service.UpdateAsync(new UpdateCategoryConfigurationRequest(1,
        [
            new("Light", "unknown-icon", 1m),
            new("Light", "light", 500m)
        ]), CancellationToken.None);
        var current = await service.GetAsync(CancellationToken.None);

        Assert.Equal(CategoryUpdateStatus.Invalid, result.Status);
        Assert.Contains("categories", result.Errors.Keys);
        Assert.Equal(1, current.Version);
        Assert.Equal(["Light", "Medium", "Heavy"], current.Categories.Select(x => x.Name));
    }

    [Fact]
    public async Task Stale_version_returns_conflict_without_overwriting_latest_configuration()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new CategoryService(database.Context);
        var first = new UpdateCategoryConfigurationRequest(1,
        [
            new("Light", "light", 0.01m),
            new("Heavy", "heavy", 1500m)
        ]);
        var stale = new UpdateCategoryConfigurationRequest(1,
        [
            new("Light", "light", 0.01m),
            new("Heavy", "heavy", 3000m)
        ]);

        await service.UpdateAsync(first, CancellationToken.None);
        var result = await service.UpdateAsync(stale, CancellationToken.None);
        var current = await service.GetAsync(CancellationToken.None);

        Assert.Equal(CategoryUpdateStatus.Conflict, result.Status);
        Assert.Equal(2, current.Version);
        Assert.Equal(1500m, current.Categories[1].StartsAtKg);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public AppDbContext Context { get; }

        private TestDatabase(SqliteConnection connection, AppDbContext context)
        {
            this.connection = connection;
            Context = context;
        }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
