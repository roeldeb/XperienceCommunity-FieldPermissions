using System.Collections.Concurrent;
using System.Text.Json;

using CMS.DataEngine;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;

using XperienceCommunity.FieldPermissions.Classes;

namespace XperienceCommunity.FieldPermissions.Services;

#pragma warning disable S2696 // Static fields are intentionally used for cross-request caching

internal class FieldPermissionService(
    IAuthenticatedUserAccessor authenticatedUserAccessor,
    IInfoProvider<FieldPermissionInfo> fieldPermissionProvider,
    IInfoProvider<RoleInfo> roleInfoProvider) : IFieldPermissionService
{
    private static readonly ConcurrentDictionary<Guid, FieldPermissionInfo> cachedPermissions = new();
    private static readonly ConcurrentDictionary<int, string> cachedRoleNames = new();
    private static bool cacheLoaded;
    private static readonly object cacheLock = new();

    public async Task<FieldRestrictionResult?> GetFieldRestrictionAsync(Guid fieldGuid)
    {
        EnsureCacheLoaded();

        if (!cachedPermissions.TryGetValue(fieldGuid, out var permission))
        {
            return null;
        }

        var user = await authenticatedUserAccessor.Get();

        if (user.IsAdministrator())
        {
            return null;
        }

        var allowedRoleIds = ParseRoleIds(permission.FieldPermissionAllowedRoles);

        if (allowedRoleIds.Any(roleId => ResolveRoleName(roleId) is { } roleName && user.IsInRole(roleName)))
        {
            return null;
        }

        string? message = string.IsNullOrWhiteSpace(permission.FieldPermissionInactiveMessage)
            ? null
            : permission.FieldPermissionInactiveMessage;

        return new FieldRestrictionResult(permission.Mode, message);
    }

    public IEnumerable<FieldPermissionInfo> GetRestrictionsForContentType(int contentTypeId) =>
        fieldPermissionProvider.Get()
            .WhereEquals(nameof(FieldPermissionInfo.FieldPermissionContentTypeID), contentTypeId)
            .ToList();

    public void InvalidateCache()
    {
        lock (cacheLock)
        {
            cachedPermissions.Clear();
            cachedRoleNames.Clear();
            cacheLoaded = false;
        }
    }

    private void EnsureCacheLoaded()
    {
        if (cacheLoaded)
        {
            return;
        }

        lock (cacheLock)
        {
            if (cacheLoaded)
            {
                return;
            }

            var allPermissions = fieldPermissionProvider.Get().ToList();

            foreach (var permission in allPermissions)
            {
                cachedPermissions[permission.FieldPermissionFieldGuid] = permission;
            }

            cacheLoaded = true;
        }
    }

    private string? ResolveRoleName(int roleId)
    {
        return cachedRoleNames.GetOrAdd(roleId, id =>
        {
            var role = roleInfoProvider.Get(id);
            return role?.RoleName ?? string.Empty;
        }) is { Length: > 0 } name ? name : null;
    }

    /// <summary>
    /// Parses role IDs from the stored value. The ObjectIdSelectorComponent stores
    /// values as a JSON array of integers, e.g. <c>[1,2,3]</c>.
    /// </summary>
    internal static IReadOnlyList<int> ParseRoleIds(string? storedValue)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<int>>(storedValue) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
