using System;

namespace Framework.BuildingBlock.Domain.Shared;

public interface IHashtExceptionHandler
{
    bool CanHandle(Exception ex);
    HashtRemoteErrorInfoDto Handle(Exception ex, bool SendExceptionsDetailsToClients, bool SendStackTraceToClients);
}
