namespace Framework.BuildingBlock.Application.Contracts;

public class ValidationMessageInfo
{
    public string MessageKey { get; set; } = null!;
    public object[] Args { get; set; } = Array.Empty<object>();
}
