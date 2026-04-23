using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Framework.BuildingBlock.HttpApi;

public interface IValidationFailureHandler
{
    object BuildValidationResponseAsync(IEnumerable<ValidationFailure> failures, HttpContext ctx, int statusCode);
}

