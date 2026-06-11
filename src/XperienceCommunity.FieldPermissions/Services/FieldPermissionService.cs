using System.Collections.Concurrent;
using System.Text.Json;

using CMS.DataEngine;
using CMS.Membership;

using Kentico.Xperience.Admin.Base.Authentication;

using XperienceCommunity.FieldPermissions.Classes;

namespace XperienceCommunity.FieldPermissions.Services;

#pragma warning disable S2696 // Static fields are intentionally used for cross-request caching

internal class FieldPermissionService(
    IAuthenticatedUserAccessor authenticatedUserAccessor,
    IInfoProvider<FieldPermissionInfo> fieldPermissionProvider,
    IInfoProvider<RoleInfo> roleInfoProvider) : IFieldPermissionService
{
    private static readonly ConcurrentDictionary<Guid, List<FieldPermissionInfo>> cachedPermissions = new();
    private static readonly ConcurrentDictionary<int, string> cachedRoleNames = new();
    private static bool cacheLoaded;
    private static readonly object cacheLock = new();

    public async Task<FieldRestrictionResult?> GetFieldRestrictionAsync(Guid fieldGuid, int? contentTypeId = null)
    {
        var applicablePermissions = GetApplicablePermissions(fieldGuid, contentTypeId);

        if (applicablePermissions.Count == 0)
        {
            return null;
        }

        var user = await authenticatedUserAccessor.Get();

        if (user.IsAdministrator())
        {
            return null;
        }

        FieldRestrictionResult? worstRestriction = null;
        bool showInCreateMode = false;

        foreach (var permission in applicablePermissions)
        {
            var roleIds = ParseRoleIds(permission.FieldPermissionAllowedRoles);
            bool userMatchesRole = roleIds.Any(roleId => ResolveRoleName(roleId) is { } roleName && user.IsInRole(roleName));
            bool isDisallowMode = string.Equals(permission.FieldPermissionRoleMode, "disallow", StringComparison.OrdinalIgnoreCase);

            // Allow mode: user must be in one of the listed roles to pass.
            // Disallow mode: user must NOT be in any of the listed roles to pass.
            bool userPassesCheck = isDisallowMode ? !userMatchesRole : userMatchesRole;

            if (userPassesCheck)
            {
                // User passes at least one permission check — field is unrestricted.
                return null;
            }

            string? message = string.IsNullOrWhiteSpace(permission.FieldPermissionInactiveMessage)
                ? null
                : permission.FieldPermissionInactiveMessage;

            // If any permission allows showing the field in create mode, the field is editable in create mode.
            showInCreateMode |= permission.FieldPermissionShowInCreateMode;

            var restriction = new FieldRestrictionResult(permission.Mode, message, showInCreateMode);

            // Track the most restrictive result (Hide > Disable).
            if (worstRestriction is null || restriction.Mode > worstRestriction.Mode)
            {
                worstRestriction = restriction;
            }
        }

        // User failed all permission checks — apply the most restrictive mode.
        // ShowInCreateMode is the logical OR across all failed permissions.
        return worstRestriction is null
            ? null
            : worstRestriction with { ShowInCreateMode = showInCreateMode };
    }

    /// <summary>
    /// Returns the permissions that apply to the given field. When the content type is known, only
    /// permissions configured for that content type are returned — fields shared across content types
    /// via reusable field schemas carry the same GUID, so the GUID alone is ambiguous. When the content
    /// type is unknown (e.g. module class forms, whose field GUIDs are unique), all permissions for the
    /// GUID are returned.
    /// </summary>
    private List<FieldPermissionInfo> GetApplicablePermissions(Guid fieldGuid, int? contentTypeId)
    {
        EnsureCacheLoaded();

        if (!cachedPermissions.TryGetValue(fieldGuid, out var permissions))
        {
            return [];
        }

        if (contentTypeId is not { } contentTypeIdValue)
        {
            return permissions;
        }

        return [.. permissions.Where(p => p.FieldPermissionContentTypeID == contentTypeIdValue)];
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
                cachedPermissions.AddOrUpdate(
                    permission.FieldPermissionFieldGuid,
                    _ => [permission],
                    (_, list) => { list.Add(permission); return list; });
            }

            cacheLoaded = true;
        }
    }

    private string? ResolveRoleName(int roleId) =>
        cachedRoleNames.GetOrAdd(roleId, id =>
        {
            var role = roleInfoProvider.Get(id);
            return role?.RoleName ?? string.Empty;
        }) is { Length: > 0 } name ? name : null;

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
