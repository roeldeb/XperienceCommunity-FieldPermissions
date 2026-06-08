using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.UIPages;

using XperienceCommunity.FieldPermissions.Admin;
using XperienceCommunity.FieldPermissions.Classes;
using XperienceCommunity.FieldPermissions.Services;

[assembly: UIPage(
    parentType: typeof(FieldPermissionList),
    slug: "create",
    uiPageType: typeof(FieldPermissionCreate),
    name: "Add field permission",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]

namespace XperienceCommunity.FieldPermissions.Admin;

/// <summary>
/// Create page for a new field permission record.
/// Sets the content type ID from the parent page context.
/// </summary>
public class FieldPermissionCreate(
    IFormComponentMapper formComponentMapper,
    IFormDataBinder formDataBinder,
    IPageLinkGenerator pageLinkGenerator,
    IFieldPermissionService fieldPermissionService)
    : CreatePage<FieldPermissionInfo, FieldPermissionEditSection>(formComponentMapper, formDataBinder, pageLinkGenerator)
{
    [PageParameter(typeof(IntPageModelBinder), typeof(ContentTypeEditSection))]
    public int ContentTypeId { get; set; }

    public override async Task ConfigurePage()
    {
        PageConfiguration.Headline = "Add field permission";

        AdditionalLinkParameters.Add(typeof(ContentTypeEditSection), ContentTypeId);

        await base.ConfigurePage();
    }

    protected override Task FinalizeInfoObject(
        FieldPermissionInfo infoObject,
        IFormFieldValueProvider fieldValueProvider,
        CancellationToken cancellationToken)
    {
        infoObject.FieldPermissionContentTypeID = ContentTypeId;
        infoObject.FieldPermissionGuid = Guid.NewGuid();

        FieldPermissionFormHelper.NormalizeBoundValues(infoObject);

        return base.FinalizeInfoObject(infoObject, fieldValueProvider, cancellationToken);
    }

    protected override async Task<ICollection<IFormItem>> GetFormItems()
    {
        var elements = await GetFormElements();
        var fields = elements.OfType<FormFieldInfo>().ToList();

        var fieldGuidField = fields.FirstOrDefault(
            f => f.Name == nameof(FieldPermissionInfo.FieldPermissionFieldGuid));

        if (fieldGuidField != null && ContentTypeId > 0)
        {
            fieldGuidField.DataType = FieldDataType.Text;
            fieldGuidField.Size = 200;
            fieldGuidField.DefaultValue = string.Empty;
            PopulateFieldOptions(fieldGuidField, ContentTypeId);
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

    protected override Task<ICommandResponse> GetSubmitSuccessResponse(
        FieldPermissionInfo savedInfoObject,
        ICollection<IFormItem> items)
    {
        fieldPermissionService.InvalidateCache();
        return base.GetSubmitSuccessResponse(savedInfoObject, items);
    }
}
