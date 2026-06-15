using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.ExceptionHandling.Localization;
using Volo.Abp.Http;
using Volo.Abp.Localization.ExceptionHandling;

namespace Framework.BuildingBlock.HttpApi;

public class FrameworkDefaultExceptionToErrorInfoConverter : DefaultExceptionToErrorInfoConverter, IExceptionToErrorInfoConverter
{
    private readonly IEnumerable<IHashtExceptionHandler> _handlers;

    public FrameworkDefaultExceptionToErrorInfoConverter(
        IOptions<AbpExceptionHandlingOptions> exceptionHandlingOptions,
        IOptions<AbpExceptionLocalizationOptions> localizationOptions,
        IStringLocalizerFactory stringLocalizerFactory,
        IStringLocalizer<AbpExceptionHandlingResource> stringLocalizer,
        IServiceProvider serviceProvider,
        IEnumerable<IHashtExceptionHandler> handlers) : base(exceptionHandlingOptions, localizationOptions, stringLocalizerFactory, stringLocalizer, serviceProvider)
    {
        _handlers = handlers;
    }

    RemoteServiceErrorInfo IExceptionToErrorInfoConverter.Convert(Exception exception, bool includeSensitiveDetails)
    {
        var exceptionHandlingOptions = CreateDefaultOptions();
        exceptionHandlingOptions.SendExceptionsDetailsToClients = includeSensitiveDetails;
        exceptionHandlingOptions.SendStackTraceToClients = includeSensitiveDetails;

        var errorInfo = CreateErrorInfoWithoutCode(exception, exceptionHandlingOptions);

        if (exception is IHasErrorCode hasErrorCodeException)
        {
            errorInfo.Code = hasErrorCodeException.Code;
        }

        return errorInfo;
    }

    RemoteServiceErrorInfo IExceptionToErrorInfoConverter.Convert(Exception exception, Action<AbpExceptionHandlingOptions> options)
    {
        var exceptionHandlingOptions = CreateDefaultOptions();
        options?.Invoke(exceptionHandlingOptions);


        var handler = _handlers.FirstOrDefault(h => !(h is DefaultExceptionHandler) && h.CanHandle(exception))
                     ?? _handlers.First(h => h is DefaultExceptionHandler);


        var errorInfo = handler.Handle(exception, exceptionHandlingOptions.SendExceptionsDetailsToClients, exceptionHandlingOptions.SendStackTraceToClients);

        if (exception is IHasErrorCode hasErrorCodeException)
        {
            errorInfo.Code = hasErrorCodeException.Code;
        }

        return ToAbpRemoteModel(errorInfo);
    }



    private RemoteServiceErrorInfo ToAbpRemoteModel(FrameworkRemoteErrorInfoDto errorInfoDto)
    {
        return new RemoteServiceErrorInfo(errorInfoDto.Message, errorInfoDto.Details, errorInfoDto.Code, errorInfoDto.Data);
    }
}
