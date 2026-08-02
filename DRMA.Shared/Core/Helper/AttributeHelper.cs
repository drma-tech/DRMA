using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DRMA.Shared.Core.Helper
{
    public static class AttributeHelper
    {
        private static readonly ConcurrentDictionary<MemberInfo, FieldSettingsAttribute> AttributeCache = new();
        private static readonly ConcurrentDictionary<Type, ResourceManager> ResourceManagers = new();

        private const string IncompleteTranslationSuffix = " (incomplete translation)";

        public static EnumFieldObject<T> GetFieldSettings<T>(this T value, bool translate = true) where T : Enum
        {
            var fieldInfo = value.GetType().GetField(value.ToString()) ?? throw new UnhandledException($"{value} field info is null");

            return fieldInfo.GetFieldSettings(value, translate);
        }

        private static EnumFieldObject<T> GetFieldSettings<T>(this MemberInfo mi, T value, bool translate = true) where T : Enum
        {
            var attr = AttributeCache.GetOrAdd(mi, x => x.GetCustomAttribute<FieldSettingsAttribute>() ?? throw new ValidationException($"Field Settings '{x.Name}' is null"));

            var obj = new EnumFieldObject<T>(attr.Name, value)
            {
                Group = attr.Group,
                Placeholder = attr.Placeholder,
                Description = attr.Description,
            };

            ApplyTranslations(obj, attr, translate);

            return obj;
        }

        private static void ApplyTranslations<T>(EnumFieldObject<T> obj, FieldSettingsAttribute attr, bool translate) where T : Enum
        {
            if (attr.ResourceType != null && translate)
            {
                var rm = ResourceManagers.GetOrAdd(attr.ResourceType, t => new ResourceManager(t.FullName!, t.Assembly));

                obj.Name = rm.GetResourceString(attr.Name) ?? throw new InvalidOperationException($"Resource not found for key: {attr.Name}");
                if (attr.Group.NotEmpty()) obj.Group = rm.GetResourceString(attr.Group);
                if (attr.Placeholder.NotEmpty()) obj.Placeholder = rm.GetResourceString(attr.Placeholder)?.Replace(@"\n", Environment.NewLine);
                if (attr.Description.NotEmpty()) obj.Description = rm.GetResourceString(attr.Description);
            }
        }

        private static string GetResourceString(this ResourceManager rm, string resourceKey)
        {
            return rm.GetString(resourceKey, CultureInfo.DefaultThreadCurrentCulture) ?? resourceKey + IncompleteTranslationSuffix;
        }
    }
}