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

        // Fixed field GUIDs: without them every FormFieldInfo gets a random GUID, so each
        // environment ends up with a different class definition (CI/CD churn).
        formInfo.GetFormField(nameof(FieldPermissionInfo.FieldPermissionID)).Guid = new Guid("0755c690-0caa-4532-8082-ffb53da8d616");

        var formItem = new FormFieldInfo
        {
            Guid = new Guid("cebefc7e-d3da-46f9-a083-fb764cfbbe78"),
            Name = nameof(FieldPermissionInfo.FieldPermissionGuid),
            DataType = FieldDataType.Guid,
            Enabled = true,
            Visible = false,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Guid = new Guid("ed1c35c0-4b22-43b5-a848-9c588ffc34b2"),
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
            Guid = new Guid("6c953561-a328-48bd-a256-123060e820aa"),
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
            Guid = new Guid("787c21b5-7bad-4d3a-9e20-1995f5fe764a"),
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
            Guid = new Guid("4bf71cd1-cef2-4de2-ade0-5219436fc54f"),
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
            Guid = new Guid("17e45b43-782f-4c22-ab6f-a0139d8f4c5e"),
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
            Guid = new Guid("fb619406-b590-4f4b-b345-a9fc488e5544"),
            Name = nameof(FieldPermissionInfo.FieldPermissionInactiveMessage),
            Caption = "Inactive message",
            DataType = FieldDataType.LongText,
            Enabled = true,
            Visible = true,
            AllowEmpty = true
        };
        formItem.Settings["controlname"] = "Kentico.Administration.TextArea";
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Guid = new Guid("fb720f69-205d-4a98-8286-85d05f15b01f"),
            Name = nameof(FieldPermissionInfo.FieldPermissionShowInCreateMode),
            Caption = "Show field in create mode",
            DataType = FieldDataType.Boolean,
            Enabled = true,
            Visible = true,
            AllowEmpty = false,
            DefaultValue = "False"
        };
        formItem.Settings["controlname"] = "Kentico.Administration.Checkbox";
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
            // OverwriteExisting heals environments installed before the fixed field GUIDs
            // existed (random GUIDs stored in the class definition).
            existingForm.CombineWithForm(form, new() { OverwriteExisting = true });
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
