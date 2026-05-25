using Framework.Localization;
using Microsoft.Extensions.Localization;

using MiniExcelLibs;
using MiniExcelLibs.Attributes;

using System.Reflection;

using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.Domain;

public interface IExcelExporter : ITransientDependency
{
    Task<MemoryStream> ExportAsync<T>(
        IEnumerable<T> data,
        IStringLocalizer L,
        string[]? ignoreColumns = null,
        bool printHeader = true,
        string sheetName = "Sheet1",
        ExcelType excelType = ExcelType.XLSX);
    Task<MemoryStream> ExportWithNestedAsync<T>(IEnumerable<T> data, IStringLocalizer L, string[]? ignoreColumns = null, int maxDepth = 2);

}

public class MiniExcelExporter : IExcelExporter
{
    public async Task<MemoryStream> ExportAsync<T>(
        IEnumerable<T> data,
        IStringLocalizer L,
        string[]? ignoreColumns = null,
        bool printHeader = true,
        string sheetName = "Sheet1",
        ExcelType excelType = ExcelType.XLSX)
    {
        var stream = new MemoryStream();

        var rows = InternalCreateData(data, L, ignoreColumns);

        await MiniExcel.SaveAsAsync(stream, rows, printHeader, sheetName, excelType);
        stream.Position = 0;
        return stream;
    }


    private List<Dictionary<string, object>> InternalCreateData<T>(IEnumerable<T> data, IStringLocalizer L, string[]? ignoreColumns = null)
    {

        var ignoreSet = new HashSet<string>(ignoreColumns ?? []);

        // Get public readable properties except ignored ones
        var props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !ignoreSet.Contains(p.Name) && p.GetCustomAttribute<ExcelIgnoreAttribute>() == null)
            .ToArray();

        // Create list of dictionaries with localized column names
        return data.Select(item =>
        {
            var dict = new Dictionary<string, object?>();
            foreach (var prop in props)
            {
                var key = prop.GetCustomAttribute<LocalizeMemberAttribute>()?.LocalizeKey ?? prop.Name;
                dict[L[key]] = prop.GetValue(item);
            }
            return dict;
        }).ToList();
    }
    public async Task<MemoryStream> ExportWithNestedAsync<T>(IEnumerable<T> data, IStringLocalizer L, string[]? ignoreColumns = null, int maxDepth = 2)
    {
        var stream = new MemoryStream();
        var ignoreSet = new HashSet<string>(ignoreColumns ?? []);

        var rows = data.Select(d => FlattenObject(d, L, ignoreSet, 0, maxDepth)).ToList();

        await MiniExcel.SaveAsAsync(stream, rows);
        stream.Position = 0;
        return stream;
    }

    private IDictionary<string, object?> FlattenObject(object? obj, IStringLocalizer L, HashSet<string> ignoreSet, int depth, int maxDepth, string? prefix = null)
    {
        var dict = new Dictionary<string, object?>();

        if (obj == null || depth > maxDepth)
            return dict;

        var type = obj.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || ignoreSet.Contains(prop.Name) || prop.CanRead && !ignoreSet.Contains(prop.Name) && prop.GetCustomAttribute<ExcelIgnoreAttribute>() == null)
                continue;

            var key = prop.GetCustomAttribute<LocalizeMemberAttribute>()?.LocalizeKey ?? prop.Name;
            var localizedKey = L[key];
            var fullKey = string.IsNullOrEmpty(prefix) ? localizedKey : $"{prefix}.{localizedKey}";

            var value = prop.GetValue(obj);

            if (value == null)
            {
                dict[fullKey] = null;
                continue;
            }

            // If simple value — add directly
            if (IsSimpleType(prop.PropertyType))
            {
                dict[fullKey] = value;
            }
            else
            {
                // Nested object — flatten recursively
                var nestedDict = FlattenObject(value, L, ignoreSet, depth + 1, maxDepth, fullKey);
                foreach (var kv in nestedDict)
                    dict[kv.Key] = kv.Value;
            }
        }

        return dict;
    }
    private static bool IsSimpleType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
            || underlying.IsEnum
            || underlying == typeof(string)
            || underlying == typeof(decimal)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateTimeOffset)
            || underlying == typeof(Guid)
            || underlying == typeof(TimeSpan);
    }
}
