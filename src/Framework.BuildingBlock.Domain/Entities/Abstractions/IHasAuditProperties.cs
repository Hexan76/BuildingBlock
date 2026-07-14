using System;

namespace Framework.BuildingBlock.Entities;

public interface IHasAuditProperties
{
    DateTime CreationTime { get; set; }

    DateTime? ModificationTime { get; set; }

    Guid? CreatorId { get; set; }

    Guid? ModifierId { get; set; }
}
