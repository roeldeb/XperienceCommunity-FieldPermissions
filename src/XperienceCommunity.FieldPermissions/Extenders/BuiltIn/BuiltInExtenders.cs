using Kentico.Xperience.Admin.Base.Forms;

using XperienceCommunity.FieldPermissions.Extenders.BuiltIn;
using XperienceCommunity.FieldPermissions.Services;

// Register extenders for all public built-in form component types.
// XbyK matches extenders by exact type — no inheritance-based matching.

[assembly: FormComponentExtender(typeof(RoleAwareTextInputExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareTextAreaExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareNumberInputExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareDecimalNumberInputExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareDropDownExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareRadioGroupExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareCheckBoxExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareDateInputExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareDateTimeInputExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareRichTextEditorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareCodeEditorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareContentItemSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareContentItemAssetUploaderExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareAssetSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareTagSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareGeneralSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareSingleGeneralSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareObjectSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareObjectIdSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareObjectGuidSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareObjectCodeNameSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareSingleObjectIdSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareContentFolderSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareIconSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareLinkExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareExtensionSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareConditionBuilderExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareSmartFolderSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareTileSelectorExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareDateTimeStringExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareTextWithLabelExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareNumberWithLabelExtender))]
[assembly: FormComponentExtender(typeof(RoleAwarePasswordExtender))]

namespace XperienceCommunity.FieldPermissions.Extenders.BuiltIn;

#pragma warning disable SA1402 // File may only contain a single type — one-liner extenders grouped intentionally

// Text & rich content
public sealed class RoleAwareTextInputExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<TextInputComponent>(svc);
public sealed class RoleAwareTextAreaExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<TextAreaComponent>(svc);
public sealed class RoleAwareRichTextEditorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<RichTextEditorComponent>(svc);
public sealed class RoleAwareCodeEditorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<CodeEditorComponent>(svc);

// Numbers
public sealed class RoleAwareNumberInputExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<NumberInputComponent>(svc);
public sealed class RoleAwareDecimalNumberInputExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<DecimalNumberInputComponent>(svc);
public sealed class RoleAwareNumberWithLabelExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<NumberWithLabelComponent>(svc);

// Selection
public sealed class RoleAwareDropDownExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<DropDownComponent>(svc);
public sealed class RoleAwareRadioGroupExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<RadioGroupComponent>(svc);
public sealed class RoleAwareCheckBoxExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<CheckBoxComponent>(svc);

// Date & time
public sealed class RoleAwareDateInputExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<DateInputComponent>(svc);
public sealed class RoleAwareDateTimeInputExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<DateTimeInputComponent>(svc);
public sealed class RoleAwareDateTimeStringExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<DateTimeStringComponent>(svc);

// Content & assets
public sealed class RoleAwareContentItemSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ContentItemSelectorComponent>(svc);
public sealed class RoleAwareContentItemAssetUploaderExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ContentItemAssetUploaderComponent>(svc);
#pragma warning disable CS0618 // AssetSelectorComponent is obsolete but still used in existing content types
public sealed class RoleAwareAssetSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<AssetSelectorComponent>(svc);
#pragma warning restore CS0618
public sealed class RoleAwareTagSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<TagSelectorComponent>(svc);
public sealed class RoleAwareContentFolderSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ContentFolderSelectorComponent>(svc);
public sealed class RoleAwareSmartFolderSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<SmartFolderSelectorComponent>(svc);

// Object selectors
public sealed class RoleAwareGeneralSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<GeneralSelectorComponent>(svc);
public sealed class RoleAwareSingleGeneralSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<SingleGeneralSelectorComponent>(svc);
public sealed class RoleAwareObjectSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ObjectSelectorComponent>(svc);
public sealed class RoleAwareObjectIdSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ObjectIdSelectorComponent>(svc);
public sealed class RoleAwareObjectGuidSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ObjectGuidSelectorComponent>(svc);
public sealed class RoleAwareObjectCodeNameSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ObjectCodeNameSelectorComponent>(svc);
public sealed class RoleAwareSingleObjectIdSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<SingleObjectIdSelectorComponent>(svc);

// Misc
public sealed class RoleAwareIconSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<IconSelectorComponent>(svc);
public sealed class RoleAwareLinkExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<LinkComponent>(svc);
public sealed class RoleAwareExtensionSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ExtensionSelectorComponent>(svc);
public sealed class RoleAwareConditionBuilderExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<ConditionBuilderComponent>(svc);
public sealed class RoleAwareTileSelectorExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<TileSelectorComponent>(svc);
public sealed class RoleAwareTextWithLabelExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<TextWithLabelComponent>(svc);
public sealed class RoleAwarePasswordExtender(IFieldPermissionService svc) : RoleAwareFormComponentExtenderBase<PasswordComponent>(svc);

#pragma warning restore SA1402
