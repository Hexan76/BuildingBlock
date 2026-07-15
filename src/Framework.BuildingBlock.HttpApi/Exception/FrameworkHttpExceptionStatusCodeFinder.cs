using System.Net;

using Framework.BuildingBlock.Domain.Shared.Exceptions;

using Microsoft.AspNetCore.Http;

using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Validation;

[Dependency(ReplaceServices = true)]
public class FrameworkHttpExceptionStatusCodeFinder : IHttpExceptionStatusCodeFinder, ITransientDependency
{
    public HttpStatusCode GetStatusCode(HttpContext httpContext, Exception exception)
    {
        return exception switch
        {
            AbpValidationException => HttpStatusCode.BadRequest,

            EntityNotFoundException => HttpStatusCode.BadRequest,

            ResourceNotFoundException => HttpStatusCode.NotFound,

            InsufficientStockException => HttpStatusCode.BadRequest,

            InvalidOrderStateException => HttpStatusCode.BadRequest,

            ConcurrencyConflictException => HttpStatusCode.Conflict,

            UpstreamServicesUnavailableException => HttpStatusCode.ServiceUnavailable,

            IUserFriendlyException => HttpStatusCode.UnprocessableContent,

            IBusinessException => HttpStatusCode.UnprocessableContent,

            AbpAuthorizationException => httpContext.User.Identity?.IsAuthenticated == true
                ? HttpStatusCode.Forbidden
                : HttpStatusCode.Unauthorized,

            _ => HttpStatusCode.InternalServerError
        };
    }
}
