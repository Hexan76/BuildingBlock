using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Framework.BuildingBlock.EntityFrameworkCore;

/// <summary>
/// Ensures a non-nullable <see cref="DateTime"/> is always persisted as UTC.
/// On write: UTC is kept as-is, Local is converted, and Unspecified is treated as UTC.
/// On read: the value is tagged with <see cref="DateTimeKind.Utc"/>.
/// Required by Npgsql for <c>timestamp with time zone</c> columns.
/// </summary>
public sealed class UtcDateTimeValueConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeValueConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc
                ? v
                : v.Kind == DateTimeKind.Local
                    ? v.ToUniversalTime()
                    : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

/// <summary>
/// Nullable counterpart of <see cref="UtcDateTimeValueConverter"/>.
/// </summary>
public sealed class UtcNullableDateTimeValueConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeValueConverter()
        : base(
            v => v.HasValue
                ? (v.Value.Kind == DateTimeKind.Utc
                    ? v.Value
                    : v.Value.Kind == DateTimeKind.Local
                        ? v.Value.ToUniversalTime()
                        : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))
                : v,
            v => v.HasValue
                ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                : v)
    {
    }
}
