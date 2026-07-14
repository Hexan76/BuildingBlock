using System.Text;

namespace Framework.BuildingBlock.Entities;

public abstract class Entity<TKey> : IEntityFramework<TKey>
{

    public TKey Id { get; set; }


    public object?[] GetKeys()
    {
        return [Id];
    }

    public virtual string? GetObjectKey()
    {
        var keys = GetKeys();
        return keys.Length switch
        {
            0 => null,
            1 when keys[0] != null => keys[0]?.ToString(),
            _ => KeyedObjectHelper.EncodeCompositeKey(keys)
        };
    }
    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}] Id = {Id}";
    }
}

public abstract class Entity : Entity<Guid>
{
}
public static class KeyedObjectHelper
{
    public static string EncodeCompositeKey(params object?[] keys)
    {
        var raw = keys.JoinAsString("||");
        var bytes = Encoding.UTF8.GetBytes(raw);
        var base64 = Convert.ToBase64String(bytes);
        var base64Url = base64
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        return base64Url;
    }

    public static string DecodeCompositeKey(string encoded)
    {
        var base64 = encoded
            .Replace("-", "+")
            .Replace("_", "/");

        switch (encoded.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        var raw = Encoding.UTF8.GetString(bytes);

        return raw;
    }
}
