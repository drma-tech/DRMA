namespace DRMA.Shared.Models;

public class Configurations
{
    public CosmosDB? CosmosDB { get; set; }
}

public class CosmosDB
{
    public string? DatabaseId { get; set; }
    public string? ConnectionString { get; set; }
}