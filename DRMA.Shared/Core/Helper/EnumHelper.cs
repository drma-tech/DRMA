namespace DRMA.Shared.Core.Helper;

public static class EnumHelper
{
    public static T[] GetValues<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>();
    }

    public static IEnumerable<EnumFieldObject<T>> GetList<T>(bool translate = true) where T : struct, Enum
    {
        var values = GetValues<T>();
        var result = new List<EnumFieldObject<T>>(values.Length);

        foreach (var val in values)
        {
            result.Add(val.GetFieldSettings(translate));
        }

        return result;
    }

    public static T ParseToEnum<T>(this string? value, T? fallback = null) where T : struct, Enum
    {
        if (Enum.TryParse<T>(value, ignoreCase: true, out var result) && Enum.IsDefined(result))
        {
            return result;
        }

        if (fallback.HasValue)
        {
            return fallback.Value;
        }

        throw new ArgumentException($"Invalid value: {value}", nameof(value));
    }
}