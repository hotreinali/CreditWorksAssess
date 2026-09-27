using CreditWorks.Api.Data;

namespace CreditWorks.Api.Domain;

public static class CategoryRanges
{
    public const decimal MinimumWeightKg = 0.01m;
    public const decimal MaximumWeightKg = 9999999999999999.99m;

    public static IReadOnlyList<string> Validate(IReadOnlyList<Category> categories)
    {
        var errors = new List<string>();
        if (categories.Count == 0)
        {
            errors.Add("At least one category is required.");
            return errors;
        }

        var ordered = categories.OrderBy(x => x.StartsAtKg).ToArray();
        if (ordered[0].StartsAtKg != MinimumWeightKg)
            errors.Add("The first category must start at 0.01 kg.");

        for (var i = 0; i < ordered.Length; i++)
        {
            var category = ordered[i];
            if (string.IsNullOrWhiteSpace(category.Name))
                errors.Add("Every category needs a name.");
            if (string.IsNullOrWhiteSpace(category.IconKey))
                errors.Add("Every category needs an icon.");
            if (decimal.Round(category.StartsAtKg, 2) != category.StartsAtKg)
                errors.Add("Category boundaries support at most two decimal places.");
            if (category.StartsAtKg < MinimumWeightKg)
                errors.Add("Category boundaries must be positive.");
            if (category.StartsAtKg > MaximumWeightKg)
                errors.Add("Category boundaries exceed the supported weight.");
            if (i > 0 && ordered[i - 1].StartsAtKg == category.StartsAtKg)
                errors.Add("Category boundaries must be unique.");
        }
        return errors;
    }

    public static Category Find(decimal weightKg, IReadOnlyList<Category> categories)
    {
        if (weightKg < MinimumWeightKg || Validate(categories).Count != 0)
            throw new ArgumentException("Weight or category configuration is invalid.");

        return categories.Where(x => x.StartsAtKg <= weightKg)
            .MaxBy(x => x.StartsAtKg)!;
    }
}
