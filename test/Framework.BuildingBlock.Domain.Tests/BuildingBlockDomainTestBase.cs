using Volo.Abp.Modularity;

namespace Framework.BuildingBlock;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class BuildingBlockDomainTestBase<TStartupModule> : BuildingBlockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
