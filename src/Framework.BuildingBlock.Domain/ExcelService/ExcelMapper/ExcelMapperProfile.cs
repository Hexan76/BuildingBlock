using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.BuildingBlock.Domain;

public abstract class ExcelMapperProfile
{
    public abstract Type ModelType { get; }
    public abstract void Configure(MapperProfileBuilder builder);
}
public class MapperProfileBuilder
{
    private readonly Dictionary<string, IColumnValueMapper> _propertyMappers = new();

    public PropertyMapBuilder ForProperty(string propertyName)
    {
        return new PropertyMapBuilder(propertyName, this);
    }

    internal void AddMapper(string propertyName, IColumnValueMapper mapper)
    {
        _propertyMappers[propertyName] = mapper;
    }

    public IReadOnlyDictionary<string, IColumnValueMapper> Build()
        => _propertyMappers;
}

public class PropertyMapBuilder
{
    private readonly string _propertyName;
    private readonly MapperProfileBuilder _builder;

    public PropertyMapBuilder(string propertyName, MapperProfileBuilder builder)
    {
        _propertyName = propertyName;
        _builder = builder;
    }

    public void Use(Func<object?, object?> converter)
    {
        _builder.AddMapper(_propertyName, new DelegateColumnValueMapper(converter));
    }

    public void Use(Func<object?, Task<object?>> asyncConverter)
    {
        _builder.AddMapper(_propertyName, new AsyncDelegateColumnValueMapper(asyncConverter));
    }
}
public class ExcelMapperConfiguration
{
    private readonly Dictionary<(Type modelType, string propertyName), IColumnValueMapper> _mappings
           = new();

    public ExcelMapperConfiguration(IEnumerable<ExcelMapperProfile> profiles)
    {
        foreach (var profile in profiles)
        {
            var builder = new MapperProfileBuilder();
            profile.Configure(builder);

            foreach (var kv in builder.Build())
            {
                _mappings[(profile.ModelType, kv.Key)] = kv.Value;
            }
        }
    }

    public IColumnValueMapper? GetMapper(Type modelType, string propertyName)
    {
        _mappings.TryGetValue((modelType, propertyName), out var mapper);
        return mapper;
    }
}