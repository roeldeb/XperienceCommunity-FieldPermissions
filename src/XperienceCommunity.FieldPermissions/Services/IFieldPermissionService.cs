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
    /// <returns>The restriction result, or null if the field is unrestricted for the current user.</returns>
    public Task<FieldRestrictionResult?> GetFieldRestrictionAsync(Guid fieldGuid);

    /// <summary>
    /// Gets all field permission configurations for a given content type.
    /// Used by the admin UI listing page.
    /// </summary>
    /// <param name="contentTypeId">The DataClassInfo.ClassID of the content type.</param>
    public IEnumerable<FieldPermissionInfo> GetRestrictionsForContentType(int contentTypeId);

    /// <summary>
    /// Invalidates the cached permission data. Call when permissions are changed.
    /// </summary>
    public void InvalidateCache();
}

/// <summary>
/// Result of a field permission check indicating the restriction to apply.
/// </summary>
/// <param name="Mode">The restriction mode (Disable or Hide).</param>
/// <param name="InactiveMessage">Optional message to display when the field is disabled.</param>
public record FieldRestrictionResult(FieldPermissionMode Mode, string? InactiveMessage);
