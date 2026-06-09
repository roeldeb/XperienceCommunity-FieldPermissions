using System.Text.Json;

using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;

using XperienceCommunity.FieldPermissions.Classes;

namespace XperienceCommunity.FieldPermissions.Admin;

/// <summary>
/// Helpers for normalizing form-bound values before a <see cref="FieldPermissionInfo"/> is persisted.
/// </summary>
/// <remarks>
/// The admin edit/create forms bind some values as their raw component types, which do not map to
/// the underlying database column types:
/// <list type="bullet">
/// <item>The <c>ObjectIdSelector</c> component binds a <see cref="List{T}"/> of role IDs into the
/// <see cref="FieldPermissionInfo.FieldPermissionAllowedRoles"/> string column.</item>
/// <item>The <c>DropDownSelector</c> component binds the selected field GUID as a string into the
/// <see cref="FieldPermissionInfo.FieldPermissionFieldGuid"/> GUID column.</item>
/// </list>
/// These must be converted to their column types in <c>FinalizeInfoObject</c> (after binding, before save),
/// otherwise the SQL insert/update fails with "No mapping exists from object type ... to a known managed provider native type".
/// </remarks>
internal static class FieldPermissionFormHelper
{
    /// <summary>
    /// Converts the raw values bound from form components into the types expected by the database columns.
    /// </summary>
    public static void NormalizeBoundValues(FieldPermissionInfo infoObject)
    {
        NormalizeAllowedRoles(infoObject);
        NormalizeFieldGuid(infoObject);
    }

    /// <summary>
    /// Resolves the human-readable name of the field referenced by <paramref name="fieldGuid"/> on the given
    /// content type, including fields from reusable field schemas. Returns <c>null</c> if it cannot be resolved.
    /// Used to render the breadcrumb/navigation label in real time (no stored display-name column).
    /// </summary>
    public static string? ResolveFieldDisplayName(int contentTypeId, Guid fieldGuid)
    {
        if (contentTypeId <= 0 || fieldGuid == Guid.Empty)
        {
            return null;
        }

        var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(contentTypeId);
        if (dataClassInfo == null)
        {
            return null;
        }

        var formInfo = FormHelper.GetFormInfo(
            ReusableFieldSchemaUtils.GetPrefixedContentTypeName(dataClassInfo.ClassName), false);
        var field = formInfo.GetFormField(fieldGuid);

        if (field == null)
        {
            return null;
        }

        return !string.IsNullOrEmpty(field.Caption)
            ? $"{field.Caption} ({field.Name})"
            : field.Name;
    }

    /// <summary>
    /// Converts the stored JSON string back into the <see cref="List{T}"/> of role IDs
    /// that the <c>ObjectIdSelector</c> component expects when binding for editing.
    /// </summary>
    public static void DenormalizeForEditing(FieldPermissionInfo infoObject) => DenormalizeAllowedRoles(infoObject);

    private static void DenormalizeAllowedRoles(FieldPermissionInfo infoObject)
    {
        object raw = infoObject.GetValue(nameof(FieldPermissionInfo.FieldPermissionAllowedRoles));

        // The ObjectIdSelector expects IEnumerable<int>; convert the stored JSON string (e.g. "[1,2,3]") back.
        if (raw is string rolesJson && !string.IsNullOrEmpty(rolesJson))
        {
            try
            {
                var roleIds = JsonSerializer.Deserialize<List<int>>(rolesJson);
                infoObject.SetValue(
                    nameof(FieldPermissionInfo.FieldPermissionAllowedRoles),
                    roleIds);
            }
            catch (JsonException)
            {
                // Value is not valid JSON; leave as-is.
            }
        }
    }

    private static void NormalizeAllowedRoles(FieldPermissionInfo infoObject)
    {
        object raw = infoObject.GetValue(nameof(FieldPermissionInfo.FieldPermissionAllowedRoles));

        // The ObjectIdSelector binds an IEnumerable<int>; persist it as the JSON the service expects (e.g. [1,2,3]).
        if (raw is IEnumerable<int> roleIds)
        {
            infoObject.SetValue(
                nameof(FieldPermissionInfo.FieldPermissionAllowedRoles),
                JsonSerializer.Serialize(roleIds));
        }
    }

    private static void NormalizeFieldGuid(FieldPermissionInfo infoObject)
    {
        object raw = infoObject.GetValue(nameof(FieldPermissionInfo.FieldPermissionFieldGuid));

        // The DropDownSelector binds the selected option (the field GUID) as a string.
        if (raw is string guidString)
        {
            infoObject.SetValue(
                nameof(FieldPermissionInfo.FieldPermissionFieldGuid),
                Guid.TryParse(guidString, out var guid) ? guid : Guid.Empty);
        }
    }
}
