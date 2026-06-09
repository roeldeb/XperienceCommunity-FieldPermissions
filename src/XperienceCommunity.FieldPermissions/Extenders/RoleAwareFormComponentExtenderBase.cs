using Kentico.Xperience.Admin.Base.Forms;

using XperienceCommunity.FieldPermissions.Services;

namespace XperienceCommunity.FieldPermissions.Extenders;

/// <summary>
/// Base class for role-aware form component extenders.
/// Checks field-level permissions and applies restrictions (disable or hide) for users without the required roles.
/// </summary>
/// <remarks>
/// XbyK matches extenders by exact form component type — there is no inheritance-based matching.
/// Each built-in form component type requires its own one-liner extender class deriving from this base.
/// For custom or third-party form components, consumers register their own extender:
/// <code>
/// [assembly: FormComponentExtender(typeof(RoleAwareMyCustomExtender))]
/// public sealed class RoleAwareMyCustomExtender(IFieldPermissionService svc)
///     : RoleAwareFormComponentExtenderBase&lt;MyCustomComponent&gt;(svc);
/// </code>
/// </remarks>
/// <typeparam name="T">The form component type to extend.</typeparam>
public abstract class RoleAwareFormComponentExtenderBase<T>(IFieldPermissionService fieldPermissionService)
    : FormComponentExtender<T> where T : IFormComponent
{
    /// <inheritdoc/>
    public override async Task ConfigureComponent()
    {
        var restriction = await fieldPermissionService.GetFieldRestrictionAsync(FormComponent.Guid);

        if (restriction is null)
        {
            return;
        }

        switch (restriction.Mode)
        {
            case FieldPermissionMode.Disable:
                if (FormComponent.Properties is FormComponentProperties props)
                {
                    props.EditMode = FormEditMode.Disabled;

                    if (!string.IsNullOrEmpty(restriction.InactiveMessage))
                    {
                        props.InactiveMessage = restriction.InactiveMessage;
                    }
                }
                break;

            case FieldPermissionMode.Hide:
                FormComponent.AddVisibilityCondition(new AlwaysHiddenVisibilityCondition());
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported {nameof(FieldPermissionMode)} value: {restriction.Mode}.");
        }
    }
}
