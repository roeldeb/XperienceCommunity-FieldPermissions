using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;

using XperienceCommunity.FieldPermissions.Admin;
using XperienceCommunity.FieldPermissions.Classes;

[assembly: UIPage(
    parentType: typeof(FieldPermissionList),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(FieldPermissionEditSection),
    name: "Edit field permission",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: UIPageOrder.NoOrder)]

namespace XperienceCommunity.FieldPermissions.Admin;

/// <summary>
/// Edit section page for a single field permission. Provides the navigation wrapper for the edit page.
/// </summary>
public class FieldPermissionEditSection : EditSectionPage<FieldPermissionInfo>
{
    /// <summary>
    /// Resolves the breadcrumb/navigation label in real time to the selected field's name
    /// (instead of the record GUID), without storing a display-name column.
    /// </summary>
    protected override Task<string> GetObjectDisplayName(BaseInfo infoObject)
    {
        if (infoObject is FieldPermissionInfo permission)
        {
            var name = FieldPermissionFormHelper.ResolveFieldDisplayName(
                permission.FieldPermissionContentTypeID, permission.FieldPermissionFieldGuid);

            if (!string.IsNullOrEmpty(name))
            {
                return Task.FromResult(name);
            }
        }

        return base.GetObjectDisplayName(infoObject);
    }
}
