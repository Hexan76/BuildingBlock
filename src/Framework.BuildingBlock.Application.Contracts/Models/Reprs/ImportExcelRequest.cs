namespace Framework.BuildingBlock.Application.Contracts;

public class ImportExcelRequest<TModel>
{
    public List<TModel> Items { get; set; } = new();
}