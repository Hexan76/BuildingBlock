//using FluentValidation;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Options;
//using System;
//using System.Linq;
//using Volo.Abp.AspNetCore.ExceptionHandling;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Http;
//using Volo.Abp.Json;

//namespace Framework.BuildingBlock.HttpApi;

//[ExposeServices(typeof(IHashtExceptionHandler))]
//public class ValidationExceptionHandler : IHashtExceptionHandler, ITransientDependency
//{
//    private readonly IExceptionToErrorInfoConverter _errorInfoConverter;
//    private readonly IHttpExceptionStatusCodeFinder _statusCodeFinder;
//    private readonly IJsonSerializer _jsonSerializer;
//    private readonly AbpExceptionHandlingOptions _options;
//    private readonly IValidationFailureHandler _validationFailureHandler;


//    public ValidationExceptionHandler(
//        IExceptionToErrorInfoConverter errorInfoConverter,
//        IHttpExceptionStatusCodeFinder statusCodeFinder,
//        IJsonSerializer jsonSerializer,
//        IOptions<AbpExceptionHandlingOptions> options,
//        IValidationFailureHandler validationFailureHandler)
//    {
//        _errorInfoConverter = errorInfoConverter;
//        _statusCodeFinder = statusCodeFinder;
//        _jsonSerializer = jsonSerializer;
//        _options = options.Value;
//        _validationFailureHandler = validationFailureHandler;
//    }

//    public bool CanHandle(Exception ex)
//    {
//        return ex is ValidationException;
//    }

//    public async Task HandleAsync(HttpContext httpContext, Exception ex)
//    {
//        var castedException = ex as ValidationException;

//        httpContext.Response.Clear();
//        httpContext.Response.StatusCode = (int)_statusCodeFinder.GetStatusCode(httpContext, ex);
//        httpContext.Response.Headers.Append(AbpHttpConsts.AbpErrorFormat, "true");
//        httpContext.Response.ContentType = "application/json";

//        var abpError = _errorInfoConverter.Convert(ex, opts =>
//        {
//            opts.SendExceptionsDetailsToClients = _options.SendExceptionsDetailsToClients;
//            opts.SendStackTraceToClients = _options.SendStackTraceToClients;
//            opts.SendExceptionDataToClientTypes = _options.SendExceptionDataToClientTypes;
//        });

//        var rejectMessage = _validationFailureHandler.BuildValidationResponseAsync(castedException.Errors, httpContext, 400);

//        await httpContext.Response.WriteAsync(_jsonSerializer.Serialize(rejectMessage));
//    }
//}
