using System;
using System.Collections.Generic;
using System.Linq;


namespace Framework.BuildingBlock.Domain;

public static class CollectionExtensions
{

    public static (List<TDto> AddedItems, List<TEntity> RemovedItems, List<(TEntity, TDto)> UpdatedItems) DiffWith<TEntity, TDto, TKey>(
        this IEnumerable<TDto> incomingDtos,
        IEnumerable<TEntity> existingEntities,
        Func<TEntity, TKey> entityKeySelector,
        Func<TDto, TKey> dtoKeySelector,
        Func<TEntity, TDto, bool> isUpdated)
        where TKey : notnull
    {
        var entityDict = existingEntities.ToDictionary(entityKeySelector);
        var dtoDict = incomingDtos.ToDictionary(dtoKeySelector);

        var added = dtoDict
            .Where(kvp => !entityDict.ContainsKey(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        var removed = entityDict
            .Where(kvp => !dtoDict.ContainsKey(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        var updated = entityDict
            .Where(kvp => dtoDict.ContainsKey(kvp.Key))
            .Select(kvp => (
                Entity: kvp.Value,
                Dto: dtoDict[kvp.Key]
            ))
            .Where(pair => isUpdated(pair.Entity, pair.Dto))
            .ToList();

        return (AddedItems: added, RemovedItems: removed, UpdatedItems: updated);
    }
}
