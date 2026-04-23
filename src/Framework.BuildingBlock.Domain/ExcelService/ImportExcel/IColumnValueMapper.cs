namespace Framework.BuildingBlock.Domain;

public interface IColumnValueMapper
{
    Task<object?> MapAsync(object? value);
}

public class DelegateColumnValueMapper : IColumnValueMapper
{
    private readonly Func<object?, object?> _converter;

    public DelegateColumnValueMapper(Func<object?, object?> converter)
    {
        _converter = converter;
    }

    public Task<object?> MapAsync(object? value)
        => Task.FromResult(_converter(value));
}

public class AsyncDelegateColumnValueMapper : IColumnValueMapper
{
    private readonly Func<object?, Task<object?>> _converter;

    public AsyncDelegateColumnValueMapper(Func<object?, Task<object?>> converter)
    {
        _converter = converter;
    }

    public Task<object?> MapAsync(object? value)
        => _converter(value);
}
