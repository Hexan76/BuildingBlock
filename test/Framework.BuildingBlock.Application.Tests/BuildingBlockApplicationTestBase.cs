using Volo.Abp.Modularity;

namespace Framework.BuildingBlock;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class BuildingBlockApplicationTestBase<TStartupModule> : BuildingBlockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
