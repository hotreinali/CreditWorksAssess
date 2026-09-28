using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CreditWorks.Api.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__CreditWorks")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__CreditWorks before running Entity Framework commands.");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connection, sql => sql.UseCompatibilityLevel(150)).Options;
        return new AppDbContext(options);
    }
}
