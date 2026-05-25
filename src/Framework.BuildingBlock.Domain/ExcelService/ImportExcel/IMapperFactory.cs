using Framework.BuildingBlock.Domain;
using Volo.Abp.DependencyInjection;

public interface IMapperFactory
{
    IColumnValueMapper? GetMapper(Type modelType, string propertyName);
}

public class DynamicMapperFactory : IMapperFactory, ITransientDependency
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<(Type modelType, string propertyName), Type> _mappers = new();

    public DynamicMapperFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Register<TModel, TMapper>(string propertyName)
        where TMapper : IColumnValueMapper
    {
        _mappers[(typeof(TModel), propertyName)] = typeof(TMapper);
    }

    public IColumnValueMapper? GetMapper(Type modelType, string propertyName)
    {
        if (_mappers.TryGetValue((modelType, propertyName), out var type))
            return (IColumnValueMapper?)_provider.GetService(type);
        return null;
    }
}
