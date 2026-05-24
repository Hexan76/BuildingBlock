using System;

namespace Framework.BuildingBlock.Domain.Shared;

public interface IExceptionLocalizationMapper
{
    Type? GetResourceType(Exception exception);
    string? GetLocalizationKey(Exception exception);
}
