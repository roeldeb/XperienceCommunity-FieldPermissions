using XperienceCommunity.FieldPermissions.Classes;

namespace XperienceCommunity.FieldPermissions.Services;

/// <summary>
/// Service for checking field-level permission restrictions for the current admin user.
/// </summary>
public interface IFieldPermissionService
{
    /// <summary>
    /// Checks whether the given field is restricted for the current admin user.
    /// Returns the restriction details if the user lacks permission, or null if unrestricted.
    /// </summary>
    /// <param name="fieldGuid">The unique GUID of the field from FormFieldInfo.</param>
    /// <param name="contentTypeId">
    /// The DataClassInfo.ClassID of the content type being edited/created, when known.
    /// Required to disambiguate fields shared across content types via reusable field schemas
    /// (such fields carry the same GUID). When null, all permissions for the GUID are considered.
    /// </param>
    /// <returns>The restriction result, or null if the field is unrestricted for the current user.</returns>
    public Task<FieldRestrictionResult?> GetFieldRestrictionAsync(Guid fieldGuid, int? contentTypeId = null);

    /// <summary>
    /// Gets all field permission configurations for a given content type (served from the cache).
    /// </summary>
    /// <param name="contentTypeId">The DataClassInfo.ClassID of the content type.</param>
    public IEnumerable<FieldPermissionInfo> GetRestrictionsForContentType(int contentTypeId);

    /// <summary>
    /// Invalidates the cached permission and role data, on every web farm instance.
    /// The cache is invalidated automatically when field permissions or roles change through the
    /// Kentico API (including the admin UI); call this only after changing the data outside of it,
    /// e.g. directly in the database.
    /// </summary>
    public void InvalidateCache();
}

/// <summary>
/// Result of a field permission check indicating the restriction to apply.
/// </summary>
/// <param name="Mode">The restriction mode (Disable or Hide).</param>
/// <param name="InactiveMessage">Optional message to display when the field is disabled.</param>
/// <param name="ShowInCreateMode">Whether the field bypasses restrictions when creating a new content item.</param>
public record FieldRestrictionResult(FieldPermissionMode Mode, string? InactiveMessage, bool ShowInCreateMode);
