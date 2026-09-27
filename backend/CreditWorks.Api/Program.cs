using CreditWorks.Api.Data;
using CreditWorks.Api.Features.Vehicles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CreditWorks")
            ?? throw new InvalidOperationException("Set ConnectionStrings__CreditWorks before starting the API."),
        sql => sql.UseCompatibilityLevel(150)));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<VehicleService>();
builder.Services.AddProblemDetails();
var app = builder.Build();

app.UseExceptionHandler();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapVehicleEndpoints();
app.Run();

public partial class Program;
