namespace Framework.Security.Abstraction;

public class TokenCacheItem
{
    public string AccessToken { get; set; }
    public DateTime Expiration { get; set; }
}