using System.Reflection;
using System.Globalization;
using Framework.BuildingBlock.Application.Contracts;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Framework.BuildingBlock.HttpApi;

public abstract class BaseEndpointWithoutRequest<TRequest, TResponse> : EndpointWithoutRequest<MessageContract<TResponse>>
    where TRequest : IRequest<MessageContract<TResponse>>, new()
    where TResponse : class
{
    public IMediator Mediator { get; set; } = null!;

    public override void Configure()
    {
        Version(1);
    }

    public override async Task<MessageContract<TResponse>> ExecuteAsync(
        CancellationToken ct)
    {
        var request = new TRequest();

        BindRouteValues(request);

        ThrowIfAnyErrors();

        return await Mediator.Send(request, ct);
    }

    private void BindRouteValues(TRequest request)
    {
        var properties = typeof(TRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var property in properties)
        {
            if (!HttpContext.Request.RouteValues.TryGetValue(
                    property.Name, out var value))
                continue;

            if (value is null)
                continue;

            var targetType = Nullable.GetUnderlyingType(property.PropertyType)
                             ?? property.PropertyType;

            var convertedValue = targetType.IsInstanceOfType(value)
                ? value
                : Convert.ChangeType(
                    value,
                    targetType,
                    CultureInfo.InvariantCulture);

            property.SetValue(request, convertedValue);
        }
    }
}