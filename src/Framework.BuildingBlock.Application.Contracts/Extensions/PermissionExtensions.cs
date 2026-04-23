using Framework.BuildingBlock.Domain.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;

namespace Framework.BuildingBlock.Application.Contracts;

public static class PermissionExtensions
{
    /// <summary>
    /// Retrieves all permissions defined in the assembly that have the HashtPermissionAttribute.
    /// </summary>
    /// <param name="assembly">The assembly to scan for permissions.</param>
    /// <returns>A list of HashtPermissionModel containing group names and permissions.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the assembly is null.</exception>
    public static List<FrameworkPermissionModel> GetHashtPermissions(this Assembly assembly, string GroupName)
    {
        var result = new List<FrameworkPermissionModel>();

        var typesWithAttribute = assembly.GetTypes()
            .Where(t => t.IsClass && t.GetCustomAttribute<FrameworkPermissionAttribute>() is not null);

        foreach (var type in typesWithAttribute)
        {
            var attr = type.GetCustomAttribute<FrameworkPermissionAttribute>();

            var permissions = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => f.GetRawConstantValue()?.ToString()!)
                .ToList();

            result.Add(new FrameworkPermissionModel
            {
                Group = $"{GroupName}.{attr.Name}",
                Permissions = permissions
            });
        }

        return result;
    }


    public static void SeedHashtPermissions(
        this PermissionGroupDefinition group,
        Assembly assembly,
        Func<string, PlainLocalizableString>? localizer = null)
    {
        localizer ??= name => PlainLocalizableString.Create(name);

        var permissionGroups = assembly.GetHashtPermissions(group.Name);

        foreach (var model in permissionGroups)
        {
            var childPermission = group.AddPermission(model.Group, localizer(model.Group));

            foreach (var permission in model.Permissions.Distinct())
            {
                childPermission.AddChild(permission, localizer(permission));
            }
        }
    }


    public static void AddHashtPermissionPolicies(
        this AuthorizationOptions options,
        IEnumerable<FrameworkPermissionModel> permissions,
        string[] authenticationSchemes = null!)
    {
        var schemes = authenticationSchemes ?? [JwtBearerDefaults.AuthenticationScheme];

        foreach (var permission in permissions.DistinctBy(p => p.Group))
        {
            // Register parent group policy (e.g. "Tax.ColorPallete")
            options.AddPolicy(permission.Group, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.Requirements.Add(new PermissionRequirement(permission.Group));
                policy.AddAuthenticationSchemes(schemes);
            });

            // Register individual permission policies (e.g. "Tax.ColorPallete.Create")
            foreach (var action in permission.Permissions.Distinct())
            {
                options.AddPolicy(action, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new PermissionRequirement(action));
                    policy.AddAuthenticationSchemes(schemes);
                });
            }
        }
    }

}