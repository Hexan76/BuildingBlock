namespace Framework.BuildingBlock.Application.Contracts;

public class BaseResponse
{
    public Guid Id { get; set; }
}
public class BaseResponse<Tkey>
{
    public Tkey Id { get; set; }
}
