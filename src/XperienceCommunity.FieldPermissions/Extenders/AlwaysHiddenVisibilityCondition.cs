using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FieldPermissions.Extenders;

/// <summary>
/// A visibility condition that always evaluates to false, hiding the field completely.
/// Used when <see cref="FieldPermissionMode.Hide"/> is configured for a restricted field.
/// </summary>
internal sealed class AlwaysHiddenVisibilityCondition : VisibilityCondition
{
    /// <inheritdoc/>
    public override bool Evaluate(IFormFieldValueProvider formFieldValueProvider) => false;
}
