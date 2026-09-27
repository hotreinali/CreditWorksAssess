namespace CreditWorks.Api.Features.Categories;

public sealed record CategoryResponse(
    int Id,
    string Name,
    string IconKey,
    decimal StartsAtKg,
    decimal? EndsBeforeKg);

public sealed record CategoryConfigurationResponse(
    int Version,
    IReadOnlyList<CategoryResponse> Categories);

public sealed record CategoryInput(
    string? Name,
    string? IconKey,
    decimal StartsAtKg);

public sealed record UpdateCategoryConfigurationRequest(
    int Version,
    IReadOnlyList<CategoryInput>? Categories);

public sealed record CategoryIconResponse(string Key, string Label);
