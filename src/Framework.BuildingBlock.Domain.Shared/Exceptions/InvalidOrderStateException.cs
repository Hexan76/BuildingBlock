using Volo.Abp;

namespace Framework.BuildingBlock.Domain.Shared.Exceptions;

public class InvalidOrderStateException : BusinessException
{
    public InvalidOrderStateException(string message)
        : base(BuildingBlockErrorCodes.InvalidOrderState, message)
    {
    }

    public InvalidOrderStateException(string orderNumber, string status)
        : base(
            BuildingBlockErrorCodes.InvalidOrderState,
            $"Order '{orderNumber}' in status '{status}' cannot be cancelled.")
    {
        OrderNumber = orderNumber;
        Status = status;
    }

    public string? OrderNumber { get; }
    public string? Status { get; }
}
