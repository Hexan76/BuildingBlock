namespace Framework.BuildingBlock.Application.Contracts;

public sealed class RejectMessage : MessageContract
{
    public IEnumerable<FrameworkValidation> Validations { get; set; }
    public Exception Exception { get; set; }

    public string? Details { get; set; }
    public string[]? ErrorsMessage { get; set; }
}