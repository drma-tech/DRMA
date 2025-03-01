using DRMA.Shared.Core.Helper;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

namespace DRMA.Shared.Core.Helper
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CustomAttribute : Attribute
    {
        public string? Group { get; set; }
        public string? Name { get; set; }
        public string? Placeholder { get; set; }
        public string? Description { get; set; }

        public Type? ResourceType { get; set; }
    }

    public static class CustomAttributeHelper
    {
        public static CustomAttribute GetCustomAttribute(this Enum value, bool translate = true)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            return fieldInfo != null ? fieldInfo.GetCustomAttribute(translate) : throw new NotificationException($"{value} fieldInfo is null");
        }

        public static CustomAttribute GetCustomAttribute<T>(this Expression<Func<T>>? expression, bool translate = true)
        {
            if (expression == null) throw new NotificationException($"{expression} expression is null");

            if (expression.Body is MemberExpression body)
            {
                return body.Member.GetCustomAttribute(translate);
            }
            else
            {
                var op = ((UnaryExpression)expression.Body).Operand;
                return ((MemberExpression)op).Member.GetCustomAttribute(translate);
            }
        }

        public static CustomAttribute GetCustomAttribute(this MemberInfo mi, bool translate = true)
        {
            if (mi.GetCustomAttribute<CustomAttribute>() is not CustomAttribute attr) throw new ValidationException($"Attribute '{mi.Name}' is null");

            if (translate && attr.ResourceType != null) //translations
            {
                var rm = new ResourceManager(attr.ResourceType.FullName ?? "", attr.ResourceType.Assembly);

                if (!string.IsNullOrEmpty(attr.Group)) attr.Group = rm.GetString(attr.Group) ?? attr.Group + " (incomplete translation)";
                if (!string.IsNullOrEmpty(attr.Name)) attr.Name = rm.GetString(attr.Name) ?? attr.Name + " (incomplete translation)";
                if (!string.IsNullOrEmpty(attr.Placeholder)) attr.Placeholder = rm.GetString(attr.Placeholder)?.Replace(@"\n", Environment.NewLine) ?? attr.Placeholder.Replace(@"\n", Environment.NewLine) + " (incomplete translation)";
                if (!string.IsNullOrEmpty(attr.Description)) attr.Description = rm.GetString(attr.Description) ?? attr.Description + " (incomplete translation)";
            }

            return attr;
        }

        public static string GetName(this Enum value, bool translate = true)
        {
            return value.GetCustomAttribute(translate).Name ?? throw new NotificationException($"{value} Name is null");
        }

        public static string GetDescription(this Enum value, bool translate = true)
        {
            return value.GetCustomAttribute(translate).Description ?? throw new NotificationException($"{value} Description is null");
        }
    }
}