namespace Framework.BuildingBlock.Domain;

public interface IExcelMapperRegistry
{
    IReadOnlyDictionary<Type, IReadOnlyDictionary<string, IColumnValueMapper>> GetProfiles();
    IReadOnlyDictionary<string, IColumnValueMapper>? GetProfileFor(Type modelType);
}

public class ExcelMapperRegistry : IExcelMapperRegistry
{
    private readonly Dictionary<Type, IReadOnlyDictionary<string, IColumnValueMapper>> _profiles = new();

    public ExcelMapperRegistry(IEnumerable<ExcelMapperProfile> profiles)
    {
        foreach (var profile in profiles)
        {
            var builder = new MapperProfileBuilder();
            profile.Configure(builder);
            _profiles[profile.ModelType] = builder.Build();
        }
    }

    public IReadOnlyDictionary<Type, IReadOnlyDictionary<string, IColumnValueMapper>> GetProfiles() => _profiles;

    public IReadOnlyDictionary<string, IColumnValueMapper>? GetProfileFor(Type modelType)
    {
        _profiles.TryGetValue(modelType, out var config);
        return config;
    }
}
