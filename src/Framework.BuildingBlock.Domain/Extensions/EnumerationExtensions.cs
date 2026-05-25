using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class EnumerationExtensions
    {
        public static IEnumerable<object> GetEnumerations(this Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"The provided type {enumType.FullName} is not an enum.");

            return Enum.GetValues(enumType)  // Use enumType directly
                .Cast<Enum>()
                .Select(e => new
                {
                    Id = Convert.ToInt32(e),
                    Name = e.ToString(),
                    Description = e.GetDescription()
                });
        }

        public static string GetDescription(this Enum @enum)
        {
            var field = @enum.GetType().GetField(@enum.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? @enum.ToString();
        }
        public static IEnumerable<Type> GetAllEnums(this Assembly assembly, Func<Type, bool>? filter = null)
        {
            return assembly
                .GetTypes()
                .Where(t => t.IsEnum && (filter == null || filter(t)));
        }

        public static IEnumerable<Type> GetAllEnums(this AppDomain appDomain, string? namespacePrefix = null)
        {
            return appDomain.GetAssemblies()
                .Where(asm =>
                    !asm.IsDynamic &&
                    asm.FullName != null &&
                    (namespacePrefix == null || asm.FullName.StartsWith(namespacePrefix)))
                .SelectMany(asm =>
                {
                    try
                    {
                        return asm.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null)!;
                    }
                })
                .Where(t => t?.IsEnum == true)
                .Cast<Type>();
        }
    }
}