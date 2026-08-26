using Framework.BuildingBlock.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Framework.BuildingBlock.Settings;

public static class SettingConfigExtension 
{
    public static ModelBuilder SettingConfigure(this ModelBuilder modelBuilder)
    {
        return modelBuilder.Entity<Setting>(builder =>
        {
            builder.ToTable(nameof(Setting));

            builder.ConfigureByConvention();
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Value).IsRequired();
        });
    }
}
