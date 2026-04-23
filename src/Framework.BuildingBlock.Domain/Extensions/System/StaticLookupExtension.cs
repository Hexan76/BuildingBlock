using Framework.BuildingBlock.Domain.Shared;
using System.Reflection;

namespace Framework.BuildingBlock.Domain;

public static class StaticLookupExtension
{
    public static IEnumerable<StaticLookupDto> ToStaticLookups(this Type enumType, Guid? tenantId = null)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException($"Type '{enumType.FullName}' is not an enum.");

        foreach (var value in Enum.GetValues(enumType))
        {
            var member = enumType.GetMember(value.ToString()!).FirstOrDefault();
            var attr = member?.GetCustomAttribute<StaticLookupAttribute>();

            var displayName = attr?.DisplayName ?? value.ToString();
            var sortOrder = attr?.Order ?? 0;
            var codeMap = attr != null && attr.CodeMap != 0 ? (int?)attr.CodeMap : null;

            yield return new StaticLookupDto
            {
                Type = enumType.Name,
                Code = Convert.ToInt32(value),
                DisplayName = displayName,
                SortOrder = sortOrder,
                CodeMap = codeMap,
                MemberName = value.ToString()
            };
        }
    }
}
