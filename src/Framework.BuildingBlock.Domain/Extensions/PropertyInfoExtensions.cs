using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System.Reflection
{
    public static class PropertyInfoExtensions
    {
        public static List<PropertyInfo> GetExcelModelProperties<TAttributeIgnore>(
                    this Type type,
                    IEnumerable<string> excludeFields = null)
            where TAttributeIgnore : Attribute
        {
            excludeFields ??= Enumerable.Empty<string>();

            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(p => p.CanWrite && p.CanRead
                                   && p.GetCustomAttribute<TAttributeIgnore>() == null
                                   && !excludeFields.Contains(p.Name))
                       .ToList();
        }
        // public static List<PropertyInfo> GetLocalizeOrderedProperties(
        //        this Type type,
        //        IEnumerable<string>? excludeFields = null)
        // {
        //     excludeFields ??= Enumerable.Empty<string>();

        //     return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        //                .Where(p => p.CanRead && p.CanWrite
        //                            && p.GetCustomAttribute<LocalizeMemberAttribute>() != null
        //                            && !excludeFields.Contains(p.Name))
        //                .OrderBy(p => p.GetCustomAttribute<LocalizeMemberAttribute>()?.Order ?? 0)
        //                .ToList();
        // }
        public static string GetFieldDescription(this PropertyInfo member)
        {
            var descriptionAttr = member.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null)
                return descriptionAttr.Description;

            return member.Name;
        }

        public static string GetFieldDisplay(this PropertyInfo member)
        {
            var descriptionAttr = member.GetCustomAttribute<DisplayNameAttribute>();
            if (descriptionAttr != null)
                return descriptionAttr.DisplayName;

            return member.Name;
        }
        // public static string GetPropertyLocalized(this PropertyInfo member)
        // {
        //     var descriptionAttr = member.GetCustomAttribute<LocalizeMemberAttribute>();
        //     if (descriptionAttr != null)
        //         return descriptionAttr.LocalizeKey;

        //     return member.Name;
        // }

        public static object GetFieldValue(this PropertyInfo member, object target)
        {
            // Get the property value from the target object
            var value = member.GetValue(target);

            if (value != null)
            {
                return value.ToString();
            }
            return string.Empty;

        }
    }
}
