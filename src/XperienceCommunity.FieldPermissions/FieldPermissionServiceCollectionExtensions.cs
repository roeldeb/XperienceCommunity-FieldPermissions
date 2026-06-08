using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using XperienceCommunity.FieldPermissions.Module;
using XperienceCommunity.FieldPermissions.Services;

namespace XperienceCommunity.FieldPermissions;

public static class FieldPermissionServiceCollectionExtensions
{
    /// <summary>
    /// Registers field permission services including the module installer and permission service.
    /// Call this in your application's <c>Program.cs</c> / <c>Startup.cs</c>.
    /// This method is idempotent and can be called multiple times safely.
    /// </summary>
    public static IServiceCollection AddFieldPermissions(this IServiceCollection services)
    {
        services.TryAddSingleton<IFieldPermissionModuleInstaller, FieldPermissionModuleInstaller>();
        services.TryAddScoped<IFieldPermissionService, FieldPermissionService>();

        return services;
    }
}
