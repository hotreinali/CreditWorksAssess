namespace CreditWorks.Api.Features.Categories;

public static class CategoryIcons
{
    public static readonly IReadOnlyList<CategoryIconResponse> All =
    [
        new("light", "Light vehicle"),
        new("medium", "Medium vehicle"),
        new("heavy", "Heavy vehicle"),
        new("car", "Car"),
        new("van", "Van"),
        new("truck", "Truck")
    ];

    public static bool Contains(string iconKey) =>
        All.Any(icon => string.Equals(icon.Key, iconKey, StringComparison.OrdinalIgnoreCase));
}
