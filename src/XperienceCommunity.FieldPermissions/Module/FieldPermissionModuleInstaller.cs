using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

using XperienceCommunity.FieldPermissions.Classes;

namespace XperienceCommunity.FieldPermissions.Module;

internal interface IFieldPermissionModuleInstaller
{
    public void Install();
}

internal class FieldPermissionModuleInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider) : IFieldPermissionModuleInstaller
{
    public void Install()
    {
        var resourceInfo = InstallModule();
        InstallFieldPermissionClass(resourceInfo);
    }

    private ResourceInfo InstallModule()
    {
        var resourceInfo = resourceInfoProvider.Get(FieldPermissionConstants.ResourceConstants.ResourceName)
                           ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = FieldPermissionConstants.ResourceConstants.ResourceDisplayName;
        resourceInfo.ResourceName = FieldPermissionConstants.ResourceConstants.ResourceName;
        resourceInfo.ResourceDescription = FieldPermissionConstants.ResourceConstants.ResourceDescription;
        resourceInfo.ResourceIsInDevelopment = FieldPermissionConstants.ResourceConstants.ResourceIsInDevelopment;

        if (resourceInfo.HasChanged)
        {
            resourceInfoProvider.Set(resourceInfo);
        }

        return resourceInfo;
    }

    private static void InstallFieldPermissionClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(FieldPermissionInfo.TYPEINFO.ObjectClassName)
                   ?? DataClassInfo.New(FieldPermissionInfo.OBJECT_TYPE);

        info.ClassName = FieldPermissionInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = FieldPermissionInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Field permission";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(FieldPermissionInfo.FieldPermissionID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(FieldPermissionInfo.FieldPermissionGuid),
            DataType = FieldDataType.Guid,
            Enabled = true,
            Visible = false,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FieldPermissionInfo.FieldPermissionContentTypeID),
            DataType = FieldDataType.Integer,
            Enabled = true,
            Visible = false,
            AllowEmpty = false,
            ReferenceToObjectType = "cms.class",
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FieldPermissionInfo.FieldPermissionFieldGuid),
            Caption = "Field",
            DataType = FieldDataType.Guid,
            Enabled = true,
            Visible = true,
            AllowEmpty = false
        };
        formItem.Settings["controlname"] = "Kentico.Administration.DropDownSelector";
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FieldPermissionInfo.FieldPermissionRoleMode),
            Caption = "Role mode",
            DataType = FieldDataType.Text,
            Size = 10,
            Enabled = true,
            Visible = true,
            AllowEmpty = false,
            DefaultValue = "allow"
        };
        formItem.Settings["controlname"] = "Kentico.Administration.RadioGroup";
        formItem.Settings["Options"] = "allow;Allow\ndisallow;Disallow";
        formItem.Settings["Inline"] = true;
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FieldPermissionInfo.FieldPermissionAllowedRoles),
            Caption = "Roles",
            DataType = FieldDataType.LongText,
            Enabled = true,
            Visible = true,
            AllowEmpty = false
        };
        formItem.Settings["controlname"] = "Kentico.Administration.ObjectIdSelector";
        formItem.Settings["ObjectType"] = "cms.role";
        formItem.Settings["MaximumItems"] = 0;
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FieldPermissionInfo.FieldPermissionMode),
            Caption = "Mode",
            DataType = FieldDataType.Integer,
            Enabled = true,
            Visible = true,
            AllowEmpty = false,
            DefaultValue = "0"
        };
        formItem.Settings["controlname"] = "Kentico.Administration.RadioGroup";
        formItem.Settings["Options"] = "0;Disable\n1;Hide";
        formItem.Settings["Inline"] = true;
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FieldPermissionInfo.FieldPermissionInactiveMessage),
            Caption = "Inactive message",
            DataType = FieldDataType.LongText,
            Enabled = true,
            Visible = true,
            AllowEmpty = true
        };
        formItem.Settings["controlname"] = "Kentico.Administration.TextArea";
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is not upserted with any existing form.
    /// </summary>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
