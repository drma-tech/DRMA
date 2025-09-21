namespace DRMA.WEB.Core.Helper;

public static class EnumHelper
{
    public static TEnum[] GetArray<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>();
    }

    public static List<EnumObject<TEnum>> GetList<TEnum>(bool translate = true) where TEnum : struct, Enum
    {
        var result = new List<EnumObject<TEnum>>();
        foreach (var val in GetArray<TEnum>())
        {
            var attr = val.GetCustomAttribute(translate);

            result.Add(new EnumObject<TEnum>(val, attr?.Name, attr?.Description, attr?.Group));
        }
        return result;
    }
}

public class EnumObject<TEnum>(TEnum value, string? name, string? description, string? group) where TEnum : struct, Enum
{
    public TEnum Value { get; set; } = value;
    public string? Name { get; set; } = name;
    public string? Description { get; set; } = description;
    public string? Group { get; set; } = group;
}