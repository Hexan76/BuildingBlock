using Volo.Abp;

namespace Framework.BuildingBlock.Domain.Shared.Exceptions;

public class InsufficientStockException : BusinessException
{
    public InsufficientStockException(long vendorId, long itemId, int requested, int available)
        : base(
            BuildingBlockErrorCodes.InsufficientStock,
            $"Insufficient stock for vendor '{vendorId}' and item '{itemId}'. Requested: {requested}, Available: {available}")
    {
        VendorId = vendorId;
        ItemId = itemId;
        Requested = requested;
        Available = available;
    }

    public long VendorId { get; }
    public long ItemId { get; }
    public int Requested { get; }
    public int Available { get; }
}
