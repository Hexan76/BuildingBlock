using Framework.BuildingBlock;
using Framework.BuildingBlock.Domain.Shared;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore;

public static class MicrosoftEfCore
{
    // Define as an extension method by adding `this` to the first parameter
    public static void RegisterEntityConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
    {
        // Debugger.Launch();
        // Get all types implementing IEntityTypeConfiguration in the given assembly
        var entityTypeConfigurationTypes = assembly.GetTypes()
               .Where(t => t.GetInterfaces().Any(i =>
                   i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
               .ToList();
        foreach (var configType in entityTypeConfigurationTypes)
        {
            // Create an instance of the configuration class
            dynamic configurationInstance = Activator.CreateInstance(configType);
            _ = modelBuilder.ApplyConfiguration(configurationInstance);
        }
    }

    public static async Task SPMigrate(this DatabaseFacade databaseFacade, string dbSchema, Assembly assembly)
    {
        var embeddedSpNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var resourceName in assembly.GetManifestResourceNames().Where(n => n.EndsWith(".sql")))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var sql = reader.ReadToEnd();

            var fileName = Path.GetFileNameWithoutExtension(resourceName);
            embeddedSpNames.Add(fileName.Split('.').Last());

            var batches = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (var batch in batches)
            {
                if (!string.IsNullOrWhiteSpace(batch))
                    await databaseFacade.ExecuteSqlRawAsync(batch);
            }
        }

        var dbSpNames = await databaseFacade
            .SqlQueryRaw<string>("SELECT name FROM sys.procedures")
            .ToListAsync();

        var toDelete = dbSpNames.Where(dbSp => !embeddedSpNames.Contains(dbSp));

        foreach (var sp in toDelete)
        {
            await databaseFacade.ExecuteSqlRawAsync($@"
            IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = '{sp}')
            DROP PROCEDURE [{dbSchema}].[{sp}];
            ");
        }
    }

    /// <summary>
    /// Applies a dynamic where clause if predicate is not null or empty.
    /// </summary>
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> source,
        string? predicate,
        params object[] values)
    {
        if (values is QueryParameter[] queryParameter)
        {
            return ApplyFilter(source, predicate, queryParameter);
        }
        if (string.IsNullOrWhiteSpace(predicate))
            return source; // no filter applied

        return source.Where(predicate, values);
    }
    /// <summary>
    /// Applies a dynamic where clause if predicate is not null or empty.
    /// </summary>
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> source,
        string? predicate)
    {
        if (string.IsNullOrWhiteSpace(predicate))
            return source; // no filter applied

        return source.Where(predicate);
    }

    /// <summary>
    /// Applies a dynamic order by clause if sortExpression is not null or empty.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string? sortExpression)
    {
        if (string.IsNullOrWhiteSpace(sortExpression))
            return source; // no sorting applied

        return source.OrderBy(sortExpression);
    }

    /// <summary>
    /// Applies a dynamic where clause if predicate is not null or empty.
    /// </summary>
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> source,
        string? predicate,
        params QueryParameter[] values)
    {
        if (string.IsNullOrWhiteSpace(predicate))
            return source; // no filter applied

        var parameters = NormalizeQueryParameters(values);
        return source.Where(predicate, parameters);
    }

    /// <summary>
    /// Converts QueryParameter[] into typed object[] suitable for Dynamic LINQ
    /// </summary>
    private static object[] NormalizeQueryParameters(QueryParameter[] queryParams)
    {
        if (queryParams is null) return queryParams;

        var result = new object[queryParams.Length];

        for (int i = 0; i < queryParams.Length; i++)
        {
            var param = queryParams[i];
            if (param == null || string.IsNullOrWhiteSpace(param.Type))
            {
                result[i] = null;
                continue;
            }
            if (param.Value is JsonElement e && e.ValueKind == JsonValueKind.Array)
            {
                var items = e.EnumerateArray().ToArray();

                result[i] = ParseCollection(param.Type, items);
                continue;
            }
            if (param != null && param.Value != null)
                result[i] = ((JsonElement)param.Value).ToString();

        }

        return result;
    }

    public static object ParseCollection(string typeName, JsonElement[] elements)
    {
        switch (typeName.ToLower())
        {
            case "int":
                return elements.Select(x => x.GetInt32()).ToArray();

            case "long":
                return elements.Select(x => x.GetInt64()).ToArray();

            case "guid":
                return elements
                    .Select(x => Guid.Parse(x.GetString()!))
                    .ToArray();

            case "string":
                return elements.Select(x => x.GetString()).ToArray();

            case "bool":
                return elements.Select(x => x.GetBoolean()).ToArray();

            default:
                throw new NotSupportedException($"Type '{typeName}' not supported");
        }
    }

    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, FilterGroup group)
    {
        if (group == null || (group.Items.Count == 0 && (group.SubGroups == null || group.SubGroups.Count == 0)))
            return query;

        var predicate = DynamicFilterBuilder.BuildPredicate<T>(group);
        return query.Where(predicate);
    }

    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, IEnumerable<FilterItem> filters)
    {
        if (filters == null || !filters.Any())
            return query;

        var group = new FilterGroup
        {
            LogicalOperator = FilterLogicalOperator.And,
            Items = filters.ToList()
        };

        var predicate = DynamicFilterBuilder.BuildPredicate<T>(group);
        return query.Where(predicate);
    }

}
