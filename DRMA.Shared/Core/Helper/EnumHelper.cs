namespace DRMA.Shared.Core.Helper;

public static class EnumHelper
{
    public static TEnum[] GetArray<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>();
    }

    public static EnumObject<TEnum>[] GetObjArray<TEnum>(bool translate = true) where TEnum : struct, Enum
    {
        return GetList<TEnum>(translate).ToArray();
    }

    public static IEnumerable<EnumObject<TEnum>> GetList<TEnum>(bool translate = true) where TEnum : struct, Enum
    {
        foreach (var val in GetArray<TEnum>())
        {
            var attr = val.GetCustomAttribute(translate);

            yield return new EnumObject<TEnum>(val, attr?.Name, attr?.Description, attr?.Group);
        }
    }
}

public class EnumObject<TEnum>(TEnum value, string? name, string? description, string? group) where TEnum : struct, Enum
{
    public TEnum Value { get; set; } = value;
    public string? Name { get; set; } = name;
    public string? Description { get; set; } = description;
    public string? Group { get; set; } = group;
}
