namespace DRMA.Shared.Models;

public class Configurations
{
    public CosmosDB? CosmosDB { get; set; }
    public Settings? Settings { get; set; }
}

public class CosmosDB
{
    public string? DatabaseId { get; set; }
    public string? ConnectionString { get; set; }
}

public class Settings
{
    public bool ShowAdSense { get; set; }
}