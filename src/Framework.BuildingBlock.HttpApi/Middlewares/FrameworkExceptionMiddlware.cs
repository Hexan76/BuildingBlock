using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http;
using Volo.Abp.Json;

namespace Framework.BuildingBlock.HttpApi;

public class FrameworkExceptionMiddlware : AbpExceptionHandlingMiddleware, ISingletonDependency
{
    private readonly Func<object, Task> _clearCacheHeadersDelegate;

    private readonly ILogger<AbpExceptionHandlingMiddleware> _logger;
    public FrameworkExceptionMiddlware(
        ILogger<AbpExceptionHandlingMiddleware> logger) : base(logger)
    {
        _logger = logger;
        _clearCacheHeadersDelegate = ClearCacheHeaders;
    }


    public async override Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("An exception occurred, but response has already started!");
                throw;
            }

            var exceptionHandlingOptions = context.RequestServices.GetRequiredService<IOptions<AbpExceptionHandlingOptions>>().Value;

            if (exceptionHandlingOptions.ShouldLogException(ex))
            {
                _logger.LogException(ex);
            }

            await context.RequestServices
                .GetRequiredService<IExceptionNotifier>()
                .NotifyAsync(new ExceptionNotificationContext(ex));

            var errorInfoConverter = context.RequestServices.GetRequiredService<IExceptionToErrorInfoConverter>();
            var statusCodeFinder = context.RequestServices.GetRequiredService<IHttpExceptionStatusCodeFinder>();
            var jsonSerializer = context.RequestServices.GetRequiredService<IJsonSerializer>();

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCodeFinder.GetStatusCode(context, ex);
            context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);
            context.Response.Headers.Append(AbpHttpConsts.AbpErrorFormat, "true");
            context.Response.Headers.Append("Content-Type", "application/json");

            var abpError = errorInfoConverter.Convert(ex, opts =>
            {
                opts.SendExceptionsDetailsToClients = exceptionHandlingOptions.SendExceptionsDetailsToClients;
                opts.SendStackTraceToClients = exceptionHandlingOptions.SendStackTraceToClients;
                opts.SendExceptionDataToClientTypes = exceptionHandlingOptions.SendExceptionDataToClientTypes;
            });

            var rejectMessage = ToRejectedMessage(abpError);

            rejectMessage.Type = MessageType.Error;
            rejectMessage.ErrorsMessage = exceptionHandlingOptions.SendStackTraceToClients
                ? ex.StackTrace?.Split('\n').Select(l => $"\t{l.Trim()}").ToArray()
                : null;

            await context.Response.WriteAsync(jsonSerializer.Serialize(ToRejectedMessage(abpError)));


        }
    }
    private Task ClearCacheHeaders(object state)
    {
        var response = (HttpResponse)state;

        response.Headers[HeaderNames.CacheControl] = "no-cache";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "-1";
        response.Headers.Remove(HeaderNames.ETag);

        return Task.CompletedTask;
    }

    private RejectMessage ToRejectedMessage(RemoteServiceErrorInfo errorInfo)
    {
        var rejected = new RejectMessage();
        rejected.Message = errorInfo.Message;
        rejected.Code = errorInfo.Code;
        rejected.Details = errorInfo.Details;
        rejected.Type = MessageType.Error;
        return rejected;
    }
}
