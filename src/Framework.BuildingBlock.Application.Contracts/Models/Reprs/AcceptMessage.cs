namespace Framework.BuildingBlock.Application.Contracts;

public sealed class AcceptMessage<TResponseMessage> : MessageContract<TResponseMessage>
where TResponseMessage : class
{
}