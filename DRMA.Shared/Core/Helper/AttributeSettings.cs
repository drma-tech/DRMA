namespace DRMA.Shared.Core.Helper;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FieldSettingsAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
    public string? Group { get; set; }
    public string? Placeholder { get; set; }
    public string? Description { get; set; }
    public Type? ResourceType { get; set; }
}