using Framework.BuildingBlock.Domain;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Framework.BuildingBlock.EntityFrameworkCore;

[ConnectionStringName(BuildingBlockDbProperties.ConnectionStringName)]
public interface IBuildingBlockDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
