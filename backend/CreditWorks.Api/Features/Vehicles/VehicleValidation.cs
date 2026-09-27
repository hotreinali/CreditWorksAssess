using CreditWorks.Api.Domain;

namespace CreditWorks.Api.Features.Vehicles;

public static class VehicleValidation
{
    public const int EarliestManufactureYear = 1886;

    public static Dictionary<string, string[]> Validate(CreateVehicleRequest request, int currentYear)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.OwnerName))
            errors[nameof(request.OwnerName)] = ["Owner's name is required."];
        else if (request.OwnerName.Trim().Length > 200)
            errors[nameof(request.OwnerName)] = ["Owner's name must be 200 characters or fewer."];

        if (request.ManufacturerId <= 0)
            errors[nameof(request.ManufacturerId)] = ["Manufacturer is required."];

        if (request.YearOfManufacture < EarliestManufactureYear || request.YearOfManufacture > currentYear + 1)
            errors[nameof(request.YearOfManufacture)] =
                [$"Year of manufacture must be between {EarliestManufactureYear} and {currentYear + 1}."];

        if (request.WeightKg < CategoryRanges.MinimumWeightKg)
            errors[nameof(request.WeightKg)] = ["Weight must be at least 0.01 kg."];
        else if (request.WeightKg > CategoryRanges.MaximumWeightKg)
            errors[nameof(request.WeightKg)] = ["Weight exceeds the supported maximum."];
        else if (decimal.Round(request.WeightKg, 2) != request.WeightKg)
            errors[nameof(request.WeightKg)] = ["Weight supports at most two decimal places."];

        return errors;
    }
}
