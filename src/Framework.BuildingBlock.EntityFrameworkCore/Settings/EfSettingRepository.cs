using Framework.BuildingBlock.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

namespace Framework.BuildingBlock.Settings;

public sealed class SettingRepository : EfGenericRepository<Setting, Guid>, ISettingRepository
{
    public SettingRepository(DbContext context) : base(context)
    {
    }

    public Task<Setting?> GetAsync(string name, Guid? tenantId, CancellationToken cancellationToken = default)
        => GetQueryable()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Name == name, cancellationToken);

    public async Task SetAsync(Setting setting, CancellationToken cancellationToken = default)
    {
        var current = await GetQueryable()
            .SingleOrDefaultAsync(x => x.Name == setting.Name, cancellationToken);

        if (current is null)
        {
            await InsertAsync(setting, true, cancellationToken);
            return;
        }

        current.Value = setting.Value;
        await UpdateAsync(current, true, cancellationToken);
    }
}
