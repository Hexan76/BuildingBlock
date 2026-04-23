using FluentValidation.Results;
using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

public class ValidationFailureHandler : IValidationFailureHandler, ISingletonDependency
{
    public object BuildValidationResponseAsync(IEnumerable<ValidationFailure> failures, HttpContext ctx, int statusCode)
    {
        return new RejectMessage()
        {
            Validations = failures.Select(
                f => new FrameworkValidation
                {
                    PropertyName = f.PropertyName,
                    AttemptedValue = f.AttemptedValue,
                    ErrorMessage = f.ErrorMessage,
                    ErrorCode = f.ErrorCode,
                    CustomState = f.CustomState,
                    FormattedMessagePlaceholderValues = f.FormattedMessagePlaceholderValues
                }).ToList(),
            Type = MessageType.Validation,
            Message = "One or more validation errors occurred.",
            Details = $"TraceId :{ctx.TraceIdentifier}",
            ErrorsMessage = [$"Request Path : {ctx.Request.Path}"]
        };
    }
}
