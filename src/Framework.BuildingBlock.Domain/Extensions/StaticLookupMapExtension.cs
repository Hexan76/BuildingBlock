using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.BuildingBlock.Domain;

public class StaticLookupMapResult<TSourceEnum, TDestEnum>
    where TSourceEnum : Enum
    where TDestEnum : Enum
{
    private readonly List<StaticLookupMapDto> _maps;
    private readonly HashSet<int> _customMappedSources = new();

    public StaticLookupMapResult(List<StaticLookupMapDto> maps)
    {
        _maps = maps;
    }

    public StaticLookupMapResult<TSourceEnum, TDestEnum> MapTo(TSourceEnum src, TDestEnum dest)
    {
        int srcValue = Convert.ToInt32(src);
        if (_customMappedSources.Contains(srcValue))
            throw new InvalidOperationException(
                $"Source member '{src}' has already been mapped using MapTo. Only one MapTo is allowed per source member.");

        var destLookup = typeof(TDestEnum).ToStaticLookups()
                           .First(l => l.Code == Convert.ToInt32(dest));

        int codeForMap = destLookup.CodeMap ?? destLookup.Code;

        // حذف همه مپ‌های پیش‌فرض برای src
        _maps.RemoveAll(m => m.Source == Convert.ToInt32(src));

        _maps.Add(new StaticLookupMapDto(Convert.ToInt32(src), Convert.ToInt32(dest))
        {
            Code = codeForMap,
            DestinationType = typeof(TDestEnum).Name
        });

        // ثبت srcValue در لیست Map سفارشی
        _customMappedSources.Add(srcValue);

        return this;
    }

    public StaticLookupMapResult<TSourceEnum, TDestEnum> Ignore(TSourceEnum src)
    {
        _maps.RemoveAll(m => m.Source == Convert.ToInt32(src));
        return this;
    }

    public List<StaticLookupMapDto> ToList() => _maps;
}

public static class StaticLookupMapper
{
    public static StaticLookupMapResult<TSourceEnum, TDestEnum> GetMap<TSourceEnum, TDestEnum>()
        where TSourceEnum : Enum
        where TDestEnum : Enum
    {
        var lookups1 = typeof(TSourceEnum).ToStaticLookups().ToList();
        var lookups2 = typeof(TDestEnum).ToStaticLookups().ToList();

        var dict2 = lookups2.ToDictionary(l => l.MemberName, l => l.Code);

        var maps = new List<StaticLookupMapDto>();

        foreach (var src in lookups1)
        {
            // MemberName مشابه
            if (dict2.TryGetValue(src.MemberName, out var destCode))
            {
                var destLookup = lookups2.First(l => l.Code == destCode);
                var codeForMap = lookups2.First(l => l.Code == destCode).CodeMap ?? destLookup.Code;
                maps.Add(new StaticLookupMapDto(src.Code, destCode)
                {
                    Code = codeForMap,
                    DestinationType = typeof(TDestEnum).Name
                });
            }
        }

        return new StaticLookupMapResult<TSourceEnum, TDestEnum>(maps);
    }
}
