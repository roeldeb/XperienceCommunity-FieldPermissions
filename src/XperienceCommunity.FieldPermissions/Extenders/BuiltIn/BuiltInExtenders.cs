using Kentico.Xperience.Admin.Base.Forms;

using XperienceCommunity.FieldPermissions.Extenders.BuiltIn;

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
public sealed class RoleAwareTextInputExtender() : RoleAwareFormComponentExtenderBase<TextInputComponent>;
public sealed class RoleAwareTextAreaExtender() : RoleAwareFormComponentExtenderBase<TextAreaComponent>;
public sealed class RoleAwareRichTextEditorExtender() : RoleAwareFormComponentExtenderBase<RichTextEditorComponent>;
public sealed class RoleAwareCodeEditorExtender() : RoleAwareFormComponentExtenderBase<CodeEditorComponent>;

// Numbers
public sealed class RoleAwareNumberInputExtender() : RoleAwareFormComponentExtenderBase<NumberInputComponent>;
public sealed class RoleAwareDecimalNumberInputExtender() : RoleAwareFormComponentExtenderBase<DecimalNumberInputComponent>;
public sealed class RoleAwareNumberWithLabelExtender() : RoleAwareFormComponentExtenderBase<NumberWithLabelComponent>;

// Selection
public sealed class RoleAwareDropDownExtender() : RoleAwareFormComponentExtenderBase<DropDownComponent>;
public sealed class RoleAwareRadioGroupExtender() : RoleAwareFormComponentExtenderBase<RadioGroupComponent>;
public sealed class RoleAwareCheckBoxExtender() : RoleAwareFormComponentExtenderBase<CheckBoxComponent>;

// Date & time
public sealed class RoleAwareDateInputExtender() : RoleAwareFormComponentExtenderBase<DateInputComponent>;
public sealed class RoleAwareDateTimeInputExtender() : RoleAwareFormComponentExtenderBase<DateTimeInputComponent>;
public sealed class RoleAwareDateTimeStringExtender() : RoleAwareFormComponentExtenderBase<DateTimeStringComponent>;

// Content & assets
public sealed class RoleAwareContentItemSelectorExtender() : RoleAwareFormComponentExtenderBase<ContentItemSelectorComponent>;
public sealed class RoleAwareContentItemAssetUploaderExtender() : RoleAwareFormComponentExtenderBase<ContentItemAssetUploaderComponent>;
#pragma warning disable CS0618 // AssetSelectorComponent is obsolete but still used in existing content types
public sealed class RoleAwareAssetSelectorExtender() : RoleAwareFormComponentExtenderBase<AssetSelectorComponent>;
#pragma warning restore CS0618
public sealed class RoleAwareTagSelectorExtender() : RoleAwareFormComponentExtenderBase<TagSelectorComponent>;
public sealed class RoleAwareContentFolderSelectorExtender() : RoleAwareFormComponentExtenderBase<ContentFolderSelectorComponent>;
public sealed class RoleAwareSmartFolderSelectorExtender() : RoleAwareFormComponentExtenderBase<SmartFolderSelectorComponent>;

// Object selectors
public sealed class RoleAwareGeneralSelectorExtender() : RoleAwareFormComponentExtenderBase<GeneralSelectorComponent>;
public sealed class RoleAwareSingleGeneralSelectorExtender() : RoleAwareFormComponentExtenderBase<SingleGeneralSelectorComponent>;
public sealed class RoleAwareObjectSelectorExtender() : RoleAwareFormComponentExtenderBase<ObjectSelectorComponent>;
public sealed class RoleAwareObjectIdSelectorExtender() : RoleAwareFormComponentExtenderBase<ObjectIdSelectorComponent>;
public sealed class RoleAwareObjectGuidSelectorExtender() : RoleAwareFormComponentExtenderBase<ObjectGuidSelectorComponent>;
public sealed class RoleAwareObjectCodeNameSelectorExtender() : RoleAwareFormComponentExtenderBase<ObjectCodeNameSelectorComponent>;
public sealed class RoleAwareSingleObjectIdSelectorExtender() : RoleAwareFormComponentExtenderBase<SingleObjectIdSelectorComponent>;

// Misc
public sealed class RoleAwareIconSelectorExtender() : RoleAwareFormComponentExtenderBase<IconSelectorComponent>;
public sealed class RoleAwareLinkExtender() : RoleAwareFormComponentExtenderBase<LinkComponent>;
public sealed class RoleAwareExtensionSelectorExtender() : RoleAwareFormComponentExtenderBase<ExtensionSelectorComponent>;
public sealed class RoleAwareConditionBuilderExtender() : RoleAwareFormComponentExtenderBase<ConditionBuilderComponent>;
public sealed class RoleAwareTileSelectorExtender() : RoleAwareFormComponentExtenderBase<TileSelectorComponent>;
public sealed class RoleAwareTextWithLabelExtender() : RoleAwareFormComponentExtenderBase<TextWithLabelComponent>;
public sealed class RoleAwarePasswordExtender() : RoleAwareFormComponentExtenderBase<PasswordComponent>;

#pragma warning restore SA1402
