using CreditWorks.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CreditWorks")
            ?? throw new InvalidOperationException("Set ConnectionStrings__CreditWorks before starting the API."),
        sql => sql.UseCompatibilityLevel(150)));
var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program;
