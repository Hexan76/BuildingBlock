using System;

namespace Framework.BuildingBlock.Entities;

public interface IHasSoftDelete
{
    bool IsDeleted { get; set; }

    DateTime? DeletionTime { get; set; }

    Guid? DeleterId { get; set; }
}
