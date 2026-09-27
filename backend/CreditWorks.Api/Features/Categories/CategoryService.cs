using CreditWorks.Api.Data;
using CreditWorks.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Api.Features.Categories;

public sealed class CategoryService(AppDbContext dbContext)
{
    public async Task<CategoryConfigurationResponse> GetAsync(CancellationToken cancellationToken)
    {
        var version = await dbContext.CategoryConfigurations.AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => x.Version)
            .SingleAsync(cancellationToken);
        var categories = await dbContext.Categories.AsNoTracking()
            .OrderBy(x => x.StartsAtKg)
            .ToListAsync(cancellationToken);

        return Map(version, categories);
    }

    public async Task<CategoryUpdateResult> UpdateAsync(
        UpdateCategoryConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var proposed = (request.Categories ?? [])
            .Select(category => new Category
            {
                Name = category.Name?.Trim() ?? string.Empty,
                IconKey = category.IconKey?.Trim().ToLowerInvariant() ?? string.Empty,
                StartsAtKg = category.StartsAtKg
            })
            .OrderBy(category => category.StartsAtKg)
            .ToArray();

        var errors = Validate(proposed);
        if (errors.Count != 0)
            return CategoryUpdateResult.Invalid(errors);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var configuration = await dbContext.CategoryConfigurations
            .SingleAsync(x => x.Id == 1, cancellationToken);
        if (configuration.Version != request.Version)
            return CategoryUpdateResult.Conflict();

        try
        {
            dbContext.Categories.RemoveRange(await dbContext.Categories.ToListAsync(cancellationToken));
            await dbContext.SaveChangesAsync(cancellationToken);

            dbContext.Categories.AddRange(proposed);
            configuration.Version++;
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return CategoryUpdateResult.Success(Map(configuration.Version, proposed));
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CategoryUpdateResult.Conflict();
        }
    }

    private static Dictionary<string, string[]> Validate(IReadOnlyList<Category> categories)
    {
        var messages = CategoryRanges.Validate(categories).ToList();
        if (categories.Any(category => category.Name.Length > 100))
            messages.Add("Category names must be 100 characters or fewer.");
        if (categories.GroupBy(category => category.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            messages.Add("Category names must be unique.");
        if (categories.Any(category => !CategoryIcons.Contains(category.IconKey)))
            messages.Add("Each category must use one of the available icons.");

        return messages.Count == 0
            ? []
            : new Dictionary<string, string[]> { ["categories"] = messages.Distinct().ToArray() };
    }

    private static CategoryConfigurationResponse Map(int version, IReadOnlyList<Category> categories)
    {
        var ordered = categories.OrderBy(category => category.StartsAtKg).ToArray();
        return new CategoryConfigurationResponse(
            version,
            ordered.Select((category, index) => new CategoryResponse(
                category.Id,
                category.Name,
                category.IconKey,
                category.StartsAtKg,
                index + 1 < ordered.Length ? ordered[index + 1].StartsAtKg : null)).ToArray());
    }
}

public enum CategoryUpdateStatus
{
    Success,
    Invalid,
    Conflict
}

public sealed record CategoryUpdateResult(
    CategoryUpdateStatus Status,
    CategoryConfigurationResponse? Configuration,
    Dictionary<string, string[]> Errors)
{
    public static CategoryUpdateResult Success(CategoryConfigurationResponse configuration) =>
        new(CategoryUpdateStatus.Success, configuration, []);

    public static CategoryUpdateResult Invalid(Dictionary<string, string[]> errors) =>
        new(CategoryUpdateStatus.Invalid, null, errors);

    public static CategoryUpdateResult Conflict() =>
        new(CategoryUpdateStatus.Conflict, null, []);
}
