namespace Framework.BuildingBlock.Application.Contracts;

public enum MessageType : byte
{
    Validation = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Message = 4,
    Success = 5,
    UnAuthorized = 6,
    Forbidden = 7,
}
