using CreditWorks.Api.Data;
using CreditWorks.Api.Domain;

namespace CreditWorks.Tests;

public sealed class CategoryRangesTests
{
    private static Category[] InitialCategories() =>
    [
        new() { Id = 1, Name = "Light", IconKey = "light", StartsAtKg = 0.01m },
        new() { Id = 2, Name = "Medium", IconKey = "medium", StartsAtKg = 500m },
        new() { Id = 3, Name = "Heavy", IconKey = "heavy", StartsAtKg = 2500m }
    ];

    [Theory]
    [InlineData("0.01", "Light")]
    [InlineData("499.99", "Light")]
    [InlineData("500.00", "Medium")]
    [InlineData("2499.99", "Medium")]
    [InlineData("2500.00", "Heavy")]
    [InlineData("999999", "Heavy")]
    public void Find_uses_inclusive_starts_and_exclusive_ends(string weight, string expected)
    {
        Assert.Equal(expected, CategoryRanges.Find(decimal.Parse(weight), InitialCategories()).Name);
    }

    [Fact]
    public void Changing_boundary_changes_existing_weight_category()
    {
        var categories = InitialCategories();
        Assert.Equal("Medium", CategoryRanges.Find(2200m, categories).Name);
        categories[2].StartsAtKg = 2000m;
        Assert.Equal("Heavy", CategoryRanges.Find(2200m, categories).Name);
    }

    [Fact]
    public void Rejects_missing_coverage_at_the_start()
    {
        var categories = InitialCategories();
        categories[0].StartsAtKg = 1m;
        Assert.NotEmpty(CategoryRanges.Validate(categories));
    }

    [Fact]
    public void Rejects_duplicate_boundaries()
    {
        var categories = InitialCategories();
        categories[1].StartsAtKg = 2500m;
        Assert.NotEmpty(CategoryRanges.Validate(categories));
    }

    [Fact]
    public void Rejects_more_than_two_decimal_places()
    {
        var categories = InitialCategories();
        categories[1].StartsAtKg = 500.001m;
        Assert.NotEmpty(CategoryRanges.Validate(categories));
    }

    [Fact]
    public void Rejects_empty_configuration()
    {
        Assert.NotEmpty(CategoryRanges.Validate([]));
    }
}
