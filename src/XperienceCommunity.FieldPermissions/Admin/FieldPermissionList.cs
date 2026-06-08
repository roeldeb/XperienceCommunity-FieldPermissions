using System.Text.Json;

using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using XperienceCommunity.FieldPermissions.Admin;
using XperienceCommunity.FieldPermissions.Classes;

[assembly: UIPage(
    parentType: typeof(ContentTypeEditSection),
    slug: "field-permissions",
    uiPageType: typeof(FieldPermissionList),
    name: "Field permissions",
    templateName: TemplateNames.LISTING,
    order: 400)]

namespace XperienceCommunity.FieldPermissions.Admin;

/// <summary>
/// Admin listing page showing all field permission configurations for a specific content type.
/// Registered as a tab within the content type edit section.
/// </summary>
public class FieldPermissionList(IInfoProvider<RoleInfo> roleInfoProvider) : ListingPage
{
    private FormInfo? formInfo;
    private Dictionary<int, string>? roleNameCache;

    protected override string ObjectType => FieldPermissionInfo.OBJECT_TYPE;

    /// <summary>
    /// Content type ID from the parent ContentTypeEditSection.
    /// </summary>
    [PageParameter(typeof(IntPageModelBinder))]
    public int ObjectId { get; set; }

    public override async Task ConfigurePage()
    {
        // Load content type's FormInfo for field name resolution
        var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(ObjectId);
        if (dataClassInfo != null)
        {
            // Use the schema-merged form info so fields from reusable field schemas resolve correctly.
            formInfo = FormHelper.GetFormInfo(
                ReusableFieldSchemaUtils.GetPrefixedContentTypeName(dataClassInfo.ClassName), false);
        }

        // Pre-load all roles for display name resolution
        roleNameCache = roleInfoProvider.Get()
            .ToDictionary(r => r.RoleID, r => r.RoleDisplayName);

        var linkParameters = new PageParameterValues
        {
            { typeof(ContentTypeEditSection), ObjectId }
        };

        // Header: Add new permission
        PageConfiguration.HeaderActions.AddLink<FieldPermissionCreate>(
            new AddLinkParameters("Add")
            {
                PageParameterValues = linkParameters
            });

        // Row actions: Edit, Delete
        PageConfiguration.AddEditRowAction<FieldPermissionEditSection>(
            new AddEditRowActionParameters { PageParameterValues = linkParameters });

        PageConfiguration.TableActions.AddDeleteAction(
            new AddDeleteActionParameters(nameof(Delete)));

        // Columns
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(FieldPermissionInfo.FieldPermissionFieldGuid),
                caption: "Field",
                sortable: false,
                formatter: (value, _) => ResolveFieldName(value))
            .AddColumn(nameof(FieldPermissionInfo.FieldPermissionAllowedRoles),
                caption: "Allowed roles",
                sortable: false,
                formatter: (value, _) => ResolveRoleNames(value))
            .AddColumn(nameof(FieldPermissionInfo.FieldPermissionMode),
                caption: "Mode",
                sortable: true,
                formatter: (value, _) => FormatMode(value))
            .AddColumn(nameof(FieldPermissionInfo.FieldPermissionInactiveMessage),
                caption: "Inactive message",
                sortable: false);

        // Filter by content type
        PageConfiguration.QueryModifiers
            .AddModifier((query, _) => query.WhereEquals(
                nameof(FieldPermissionInfo.FieldPermissionContentTypeID), ObjectId));

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id)
    {
        var provider = FieldPermissionInfo.Provider;
        var info = provider.Get(id);

        if (info != null)
        {
            provider.Delete(info);
        }

        return Task.FromResult(ResponseFrom(new RowActionResult(false)));
    }

    private string ResolveFieldName(object value)
    {
        if (value is Guid guid && formInfo != null)
        {
            var field = formInfo.GetFormField(guid);
            if (field != null)
            {
                return !string.IsNullOrEmpty(field.Caption)
                    ? $"{field.Caption} ({field.Name})"
                    : field.Name;
            }
        }

        return value.ToString() ?? string.Empty;
    }

    private static string FormatMode(object value) =>
        value is int mode && mode == (int)FieldPermissionMode.Hide
            ? "Hide"
            : "Disable";

    private string ResolveRoleNames(object value)
    {
        if (value is not string json || string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            var roleIds = JsonSerializer.Deserialize<List<int>>(json);
            if (roleIds is null || roleIds.Count == 0)
            {
                return string.Empty;
            }

            var names = roleIds
                .Select(id => roleNameCache?.TryGetValue(id, out string? name) == true ? name : $"#{id}")
                .OrderBy(n => n);

            return string.Join(", ", names);
        }
        catch (JsonException)
        {
            return value.ToString() ?? string.Empty;
        }
    }
}
