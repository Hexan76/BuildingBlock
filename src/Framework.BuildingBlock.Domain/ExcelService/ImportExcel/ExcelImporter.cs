using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using System.Data;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.Domain;

public interface IExcelImporter<TModel>
    where TModel : class, new()
{
    Task<List<TModel>> ImportAsync(ExcelImportConfig config, DataTable sheetData);
    Task<List<TModel>> ImportByStreamToModel(ExcelImportConfig config, Stream stream, bool useHeaderRow = true);
    Task<DataTable> ReadExcelToDataTableAsync(Stream stream, bool useHeaderRow = true);
}

public class ExcelImporter<TModel> : IExcelImporter<TModel>, ITransientDependency
    where TModel : class, new()
{
    private readonly ExcelMapperConfiguration _mapperConfiguration;

    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<ExcelImporter<TModel>> _logger;

    public ExcelImporter(ILogger<ExcelImporter<TModel>> logger, ExcelMapperConfiguration mapperConfiguration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _mapperConfiguration = mapperConfiguration;
        _serviceProvider = serviceProvider;
    }

    public async Task<List<TModel>> ImportAsync(ExcelImportConfig config, DataTable sheetData)
    {
        var result = new List<TModel>();

        foreach (DataRow row in sheetData.Rows)
        {
            var model = new TModel();

            foreach (var col in config.Columns)
            {
                var property = typeof(TModel).GetProperty(col.PropertyName);
                if (property == null)
                {
                    _logger.LogWarning($"Property {col.PropertyName} not found in {typeof(TModel).Name}");
                    continue;
                }

                object? value = row[col.ColumnIndex];
                object? mappedValue = value;

                var mapper = _mapperConfiguration.GetMapper(typeof(TModel), col.PropertyName);
                if (mapper != null)
                {
                    try
                    {
                        mappedValue = await mapper.MapAsync(value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Custom mapper failed for {col.PropertyName}");
                    }
                }

                try
                {
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    if (mappedValue != null && targetType != mappedValue.GetType())
                        mappedValue = Convert.ChangeType(mappedValue, targetType);

                    property.SetValue(model, mappedValue);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to set {property.Name} with value {mappedValue}");
                }
            }

            result.Add(model);
        }

        return result;
    }
    public async Task<DataTable> ReadExcelToDataTableAsync(Stream stream, bool useHeaderRow = true)
    {
        var dt = new DataTable();

        var rows = await MiniExcel.QueryAsync(stream, useHeaderRow);

        var rowList = rows.Select(r => ((IDictionary<string, object>)r).Values.ToArray()).ToList();

        if (!rowList.Any())
            return dt;

        for (int i = 0; i < rowList[0].Length; i++)
            dt.Columns.Add($"Col{i}");

        foreach (var row in rowList)
            dt.Rows.Add(row);

        return dt;
    }

    public async Task<List<TModel>> ImportByStreamToModel(ExcelImportConfig config, Stream stream, bool useHeaderRow = true)
    {
        var dt = await ReadExcelToDataTableAsync(stream, useHeaderRow);

        return await ImportAsync(config, dt);
    }
}
