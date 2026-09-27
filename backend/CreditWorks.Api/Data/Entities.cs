namespace CreditWorks.Api.Data;

public sealed class Manufacturer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<Vehicle> Vehicles { get; set; } = [];
}

public sealed class Vehicle
{
    public int Id { get; set; }
    public required string OwnerName { get; set; }
    public int ManufacturerId { get; set; }
    public Manufacturer Manufacturer { get; set; } = null!;
    public int YearOfManufacture { get; set; }
    public decimal WeightKg { get; set; }
}

public sealed class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string IconKey { get; set; }
    public decimal StartsAtKg { get; set; }
}

public sealed class CategoryConfiguration
{
    public int Id { get; set; }
    public int Version { get; set; }
}
