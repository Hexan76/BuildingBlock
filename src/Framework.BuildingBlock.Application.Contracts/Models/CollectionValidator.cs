using FluentValidation;

namespace Framework.BuildingBlock.Application.Contracts;

public class CollectionValidator<T> : AbstractValidator<List<T>> where T : IRowNumber
{
    public CollectionValidator(IValidator<T> itemValidator)
    {
        RuleForEach(x => x).SetValidator(itemValidator);
    }
}
public interface IRowNumber
{
    int RowNumber { get; set; }
}
