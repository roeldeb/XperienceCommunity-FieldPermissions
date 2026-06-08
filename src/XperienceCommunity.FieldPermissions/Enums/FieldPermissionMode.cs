namespace XperienceCommunity.FieldPermissions;

/// <summary>
/// Defines the behavior when a user lacks permission for a field.
/// </summary>
public enum FieldPermissionMode
{
    /// <summary>
    /// The field is shown but disabled (read-only) with an optional message.
    /// </summary>
    Disable = 0,

    /// <summary>
    /// The field is completely hidden from the form.
    /// </summary>
    Hide = 1
}
