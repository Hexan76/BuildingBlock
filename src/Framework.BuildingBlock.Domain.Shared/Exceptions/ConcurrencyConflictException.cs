using Volo.Abp;

namespace Framework.BuildingBlock.Domain.Shared.Exceptions;

public class ConcurrencyConflictException : BusinessException
{
    public ConcurrencyConflictException(string message)
        : base(BuildingBlockErrorCodes.ConcurrencyConflict, message)
    {
    }
}
