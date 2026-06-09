using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

using XperienceCommunity.FieldPermissions.Admin;
using XperienceCommunity.FieldPermissions.Classes;
using XperienceCommunity.FieldPermissions.Services;

[assembly: UIPage(
    parentType: typeof(FieldPermissionEditSection),
    slug: "general",
    uiPageType: typeof(FieldPermissionEdit),
    name: "General",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]

namespace XperienceCommunity.FieldPermissions.Admin;

/// <summary>
/// Edit page for an existing field permission record.
/// </summary>
public class FieldPermissionEdit(
    IFormComponentMapper formComponentMapper,
    IFormDataBinder formDataBinder,
    IFieldPermissionService fieldPermissionService)
    : InfoEditPage<FieldPermissionInfo>(formComponentMapper, formDataBinder)
{
    [PageParameter(typeof(IntPageModelBinder), typeof(FieldPermissionEditSection))]
    public override int ObjectId { get; set; }

    public override async Task ConfigurePage()
    {
        PageConfiguration.Headline = "Edit field permission";
        await base.ConfigurePage();
    }

    protected override async Task<FieldPermissionInfo> GetInfoObject(CancellationToken? cancellationToken = null)
    {
        var info = await base.GetInfoObject(cancellationToken);
        FieldPermissionFormHelper.DenormalizeForEditing(info);
        return info;
    }

    protected override Task FinalizeInfoObject(
        FieldPermissionInfo infoObject,
        IFormFieldValueProvider fieldValueProvider,
        CancellationToken cancellationToken)
    {
        FieldPermissionFormHelper.NormalizeBoundValues(infoObject);

        return base.FinalizeInfoObject(infoObject, fieldValueProvider, cancellationToken);
    }

    protected override async Task<ICommandResponse> GetSubmitSuccessResponse(
        FieldPermissionInfo savedInfoObject,
        ICollection<IFormItem> items)
    {
        fieldPermissionService.InvalidateCache();

        string? displayName = FieldPermissionFormHelper.ResolveFieldDisplayName(
            savedInfoObject.FieldPermissionContentTypeID, savedInfoObject.FieldPermissionFieldGuid);

        if (string.IsNullOrEmpty(displayName))
        {
            return await base.GetSubmitSuccessResponse(savedInfoObject, items);
        }

        // Keep the breadcrumb/navigation label in sync with the resolved field name after saving.
        var result = new EditPageSuccessFormSubmissionResult
        {
            Items = await items.OnlyVisible().GetClientProperties(),
            ObjectDisplayName = displayName,
            ObjectId = savedInfoObject.FieldPermissionID,
            RefetchAll = RefetchAll
        };

        return ResponseFrom(result).AddSuccessMessage(LocalizationService?.GetString("base.forms.saved"));
    }

    protected override async Task<ICollection<IFormItem>> GetFormItems()
    {
        var elements = await GetFormElements();
        var fields = elements.OfType<FormFieldInfo>().ToList();

        var fieldGuidField = fields.FirstOrDefault(
            f => f.Name == nameof(FieldPermissionInfo.FieldPermissionFieldGuid));

        if (fieldGuidField != null && ObjectId > 0)
        {
            fieldGuidField.DataType = FieldDataType.Text;
            fieldGuidField.Size = 200;
            fieldGuidField.DefaultValue = string.Empty;

            var infoObject = await FieldPermissionInfo.Provider.GetAsync(ObjectId);
            if (infoObject != null)
            {
                PopulateFieldOptions(fieldGuidField, infoObject.FieldPermissionContentTypeID);
            }
        }

        var modeField = fields.FirstOrDefault(
            f => f.Name == nameof(FieldPermissionInfo.FieldPermissionMode));

        if (modeField != null)
        {
            modeField.DataType = FieldDataType.Text;
            modeField.Size = 10;
            modeField.DefaultValue = "0";
        }

        var formItems = GetFormComponents(fields).ToList<IFormItem>();
        FormItemCollectionProvider.AddCategories(elements, formItems);

        return formItems;
    }

    private static void PopulateFieldOptions(FormFieldInfo fieldInfo, int contentTypeId)
    {
        var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(contentTypeId);
        if (dataClassInfo == null)
        {
            return;
        }

        // Use the schema-merged form info so fields from reusable field schemas are included.
        var formInfo = FormHelper.GetFormInfo(
            ReusableFieldSchemaUtils.GetPrefixedContentTypeName(dataClassInfo.ClassName), false);
        var options = formInfo.GetFields<FormFieldInfo>()
            .Where(f => !string.IsNullOrEmpty(f.Name) && f.Visible)
            .Select(f =>
            {
                string label = string.IsNullOrEmpty(f.Caption) ? f.Name : $"{f.Caption} ({f.Name})";
                return $"{f.Guid};{label}";
            });

        fieldInfo.Settings["Options"] = string.Join("\n", options);
    }
}
