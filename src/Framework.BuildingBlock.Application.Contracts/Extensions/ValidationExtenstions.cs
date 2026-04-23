using Framework.BuildingBlock.Application.Contracts;
using Microsoft.Extensions.Localization;

namespace FluentValidation.Results;

public static class ValidationExtensions
{
    public static async Task ValidateCollectionAsync<T>(
        this IEnumerable<T> items,
        IValidator<T> itemValidator,
        IStringLocalizer L,
        CancellationToken cancellationToken = default) where T : IRowNumber
    {
        var list = items.ToList();
        for (int i = 0; i < list.Count; i++)
            list[i].RowNumber = i + 1;

        var collectionValidator = new CollectionValidator<T>(itemValidator);
        var validationResult = await collectionValidator.ValidateAsync(list, cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                error.ErrorMessage = L["Texts.Validations.RowNumberError", error.CustomState, error.ErrorMessage ?? ""];
            }

            throw new ValidationException(validationResult.Errors);
        }
    }
}
