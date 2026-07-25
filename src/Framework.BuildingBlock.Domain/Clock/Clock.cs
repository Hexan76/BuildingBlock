using Framework.BuildingBlock.Abstracts;

using Microsoft.Extensions.Options;

namespace Framework.BuildingBlock.Clock;

public sealed class Clock : IClock
{
    private readonly ClockOptions _options;

    public Clock(IOptions<ClockOptions> options)
    {
        _options = options.Value;
    }


    public DateTime Now
    {
        get
        {
            return _options.Kind switch
            {
                DateTimeKind.Utc => DateTime.UtcNow,
                DateTimeKind.Local => DateTime.Now,
                _ => DateTime.UtcNow
            };
        }
    }


    public DateTime Normalize(DateTime dateTime)
    {
        if (_options.Kind == DateTimeKind.Utc)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,

                DateTimeKind.Local =>
                    dateTime.ToUniversalTime(),

                DateTimeKind.Unspecified =>
                    DateTime.SpecifyKind(
                        dateTime,
                        DateTimeKind.Utc),

                _ => dateTime
            };
        }


        return dateTime.Kind switch
        {
            DateTimeKind.Unspecified =>
                DateTime.SpecifyKind(
                    dateTime,
                    DateTimeKind.Local),

            _ => dateTime
        };
    }


    public DateTime? Normalize(DateTime? dateTime)
    {
        return dateTime.HasValue
            ? Normalize(dateTime.Value)
            : null;
    }
}
