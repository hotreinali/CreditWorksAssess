using CreditWorks.Api.Features.Vehicles;

namespace CreditWorks.Tests;

public sealed class VehicleValidationTests
{
    [Fact]
    public void Accepts_valid_vehicle()
    {
        var request = new CreateVehicleRequest("Jane Turei", 2, 2020, 1850.75m);

        Assert.Empty(VehicleValidation.Validate(request, 2026));
    }

    [Fact]
    public void Rejects_missing_and_out_of_range_values()
    {
        var request = new CreateVehicleRequest(" ", 0, 1885, 0m);

        var errors = VehicleValidation.Validate(request, 2026);

        Assert.Contains(nameof(request.OwnerName), errors.Keys);
        Assert.Contains(nameof(request.ManufacturerId), errors.Keys);
        Assert.Contains(nameof(request.YearOfManufacture), errors.Keys);
        Assert.Contains(nameof(request.WeightKg), errors.Keys);
    }

    [Fact]
    public void Rejects_weight_with_more_than_two_decimal_places()
    {
        var request = new CreateVehicleRequest("Jane Turei", 2, 2020, 1850.751m);

        Assert.Contains(nameof(request.WeightKg), VehicleValidation.Validate(request, 2026).Keys);
    }

    [Theory]
    [InlineData(2027, true)]
    [InlineData(2028, false)]
    public void Allows_at_most_next_year_models(int year, bool isValid)
    {
        var request = new CreateVehicleRequest("Jane Turei", 2, year, 1200m);

        Assert.Equal(isValid, VehicleValidation.Validate(request, 2026).Count == 0);
    }
}
