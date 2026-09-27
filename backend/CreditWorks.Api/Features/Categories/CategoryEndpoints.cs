namespace CreditWorks.Api.Features.Categories;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/category-icons", () => Results.Ok(CategoryIcons.All));

        endpoints.MapGet("/api/categories", async (
            CategoryService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(cancellationToken)));

        endpoints.MapPut("/api/categories", async (
            UpdateCategoryConfigurationRequest request,
            CategoryService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.UpdateAsync(request, cancellationToken);
            return result.Status switch
            {
                CategoryUpdateStatus.Success => Results.Ok(result.Configuration),
                CategoryUpdateStatus.Invalid => Results.ValidationProblem(result.Errors),
                CategoryUpdateStatus.Conflict => Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Category configuration has changed",
                    detail: "Reload the latest categories before saving again."),
                _ => throw new InvalidOperationException("Unexpected category update result.")
            };
        });

        return endpoints;
    }
}
