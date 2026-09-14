using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

[ExposeServices(typeof(IHashtExceptionHandler))]
public class AuthorizationExceptionHandler : IHashtExceptionHandler, ITransientDependency
{
    private readonly IStringLocalizer<BuildingBlockResource> L;

    public AuthorizationExceptionHandler(
        IStringLocalizer<BuildingBlockResource> localizer)
    {
        L = localizer;
    }

    public bool CanHandle(Exception ex)
    {
        return ex is AbpAuthorizationException
               || ex is AuthenticationException
               || ex is UnauthorizedAccessException
               || ex is SecurityTokenValidationException
               || ex is SecurityTokenException;
    }

    public FrameworkRemoteErrorInfoDto Handle(
        Exception ex,
        bool sendExceptionsDetailsToClients,
        bool sendStackTraceToClients)
    {
        if (IsAuthenticationException(ex))
        {
            return CreateError(
                L["Auth:Unauthorized"],
                "401",
                ex,
                sendExceptionsDetailsToClients);
        }

        if (ex is AbpAuthorizationException)
        {
            return CreateError(
                L["Auth:Forbidden"],
                "403",
                ex,
                sendExceptionsDetailsToClients);
        }

        return CreateError(
            L["Auth:Unauthorized"],
            "401",
            ex,
            sendExceptionsDetailsToClients);
    }

    // ---------------- Private ----------------

    private static bool IsAuthenticationException(Exception ex)
    {
        return ex is AuthenticationException
               || ex is SecurityTokenException
               || ex is SecurityTokenValidationException
               || ex is UnauthorizedAccessException;
    }

    private static FrameworkRemoteErrorInfoDto CreateError(
        string message,
        string code,
        Exception ex,
        bool sendDetails)
    {
        return new FrameworkRemoteErrorInfoDto(message, code)
        {
            Details = sendDetails ? ex.ToString() : null
        };
    }
}