using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DRMA.Shared.Core.Helper
{
    public static class ClassHelper
    {
        public static T? GetClonedInstance<T>(this T? instance) where T : class
        {
            if (instance == null) return null;
            var json = JsonSerializer.Serialize(instance, options: new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles });
            if (string.IsNullOrEmpty(json)) throw new UnhandledException("invalid instance for GetClonedInstance");
            var result = JsonSerializer.Deserialize<T>(json);
            return result ?? throw new UnhandledException("invalid instance for GetClonedInstance");
        }

        public static void ShallowCopyEntity<T>(T source, T dest, params string[] except)
        {
            var props = typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props.Where(x => !except.Contains(x.Name) && IsValueTypeOrString(x.PropertyType)))
            {
                var getMethod = prop.GetGetMethod(false);
                if (getMethod == null) continue;
                var setMethod = prop.GetSetMethod(false);
                if (setMethod == null) continue;
                prop.SetValue(dest, prop.GetValue(source));
            }
        }

        private static bool IsValueTypeOrString(Type type)
        {
            return type.IsSubclassOf(typeof(ValueType)) || type == typeof(string);
        }
    }
}