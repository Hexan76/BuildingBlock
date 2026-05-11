namespace Framework.Security.Abstraction;
public interface ITokenProvider
{
    Task<string> GetTokenAsync(string clientId, string[] scopes);
}