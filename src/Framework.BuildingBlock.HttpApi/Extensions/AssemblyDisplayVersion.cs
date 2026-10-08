using System.Reflection;

namespace Framework.BuildingBlock.HttpApi;

public static class AssemblyDisplayVersion
{
    public static string Read(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plusIndex = informational.IndexOf('+');
            return plusIndex >= 0 ? informational[..plusIndex] : informational;
        }

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }
}
