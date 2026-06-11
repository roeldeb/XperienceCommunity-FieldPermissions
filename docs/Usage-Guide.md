# Usage Guide

This guide provides full documentation for the **XperienceCommunity.FieldPermissions** library.

## Table of Contents

- [Installation](#installation)
- [How It Works](#how-it-works)
- [Service Registration](#service-registration)
- [Admin UI Configuration](#admin-ui-configuration)
- [Extending Custom Form Components](#extending-custom-form-components)
- [Key Types](#key-types)
- [Database Structure](#database-structure)
- [Troubleshooting](#troubleshooting)

---

## Installation

```powershell
dotnet add package XperienceCommunity.FieldPermissions
```

## How It Works

The package uses `FormComponentExtender<T>`, the official Xperience by Kentico extension point for modifying form components at render time.

**Enforcement flow:**

1. An admin configures a field restriction via the "Field permissions" tab on a content type
2. The restriction is stored in the `XperienceCommunity_FieldPermission` database table
3. When a user opens a content item for editing, each form component's extender checks the current user's roles against the stored restrictions
4. Whether the user is restricted depends on the **role mode**:
   - **Allow** — only users in one of the selected roles may edit the field; everyone else is restricted
   - **Disallow** — users in any of the selected roles are restricted; everyone else may edit
5. When restricted, the field is either **disabled** (read-only with an optional message) or **hidden** entirely, based on the restriction mode

**Matching by field GUID:** Each field in a content type has a unique GUID that is immutable — it never changes even if the field is renamed. The package uses this GUID to match restrictions to fields, so renaming a field in the admin UI will not break existing permissions.

**Administrator bypass:** Users with the Administrator role always have full access to all fields, regardless of any configured restrictions.

## Service Registration

Register the services in your `Program.cs`:

```csharp
using XperienceCommunity.FieldPermissions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKentico();
builder.Services.AddFieldPermissions();

var app = builder.Build();
```

The `AddFieldPermissions()` method:

- Registers the module installer (creates the database table on first run)
- Registers the `IFieldPermissionService` for permission checks
- Is idempotent — safe to call multiple times

## Admin UI Configuration

Navigate to **Content types** → select a content type → **Field permissions** tab.

From there you can:

1. **Add** a new field permission using the "Add" button
2. **Select a field** from the content type's form definition (includes fields from reusable field schemas)
3. **Choose the role mode:**
   - **Allow** — only the selected roles may edit the field
   - **Disallow** — the selected roles may **not** edit the field; everyone else may
4. **Select roles** the rule applies to (using the role selector)
5. **Choose the restriction mode:**
   - **Disable** — field is shown as read-only with an optional message
   - **Hide** — field is completely invisible
6. **Set an inactive message** (optional) — shown to restricted users when mode is "Disable"
7. **Show field in create mode** (optional) — when enabled, the field's restrictions are bypassed while a new item is being created, so a restricted (e.g. required) field can still be filled in. Restrictions apply as normal once the item is saved and subsequently edited. When multiple rules target the same field, the field is shown in create mode if **any** of them has this enabled.

The listing shows all configured restrictions for the content type with the resolved field name, role names, mode, and message.

### Fields shared by reusable field schemas

Content types that use the same **reusable field schema** include the schema's fields with the **same field GUID**. Permissions are stored per content type, and the correct content type is resolved at runtime so a rule configured for one content type does **not** leak onto the identical field in another. The content type is derived from the admin request (the posted command data for create flows, or the object identifier in the page path for edit). This works for website channel pages, content hub items, and module class entries; module class fields have unique GUIDs and need no disambiguation.

## Extending Custom Form Components

The package includes built-in extenders for all standard Xperience form components (text inputs, dropdowns, rich text, selectors, etc.).

If your project uses **custom form components**, you need to register an extender for each one. This is a one-liner:

```csharp
using XperienceCommunity.FieldPermissions.Extenders;

using Kentico.Xperience.Admin.Base.Forms;

[assembly: FormComponentExtender(typeof(RoleAwareCustomDropdownExtender))]

namespace MyProject;

// Inherit the base class — that's it. No DI parameters needed.
public sealed class RoleAwareCustomDropdownExtender()
    : RoleAwareFormComponentExtenderBase<CustomDropdownComponent>;
```

> **⚠️ Breaking change when upgrading from 1.0.0.** `RoleAwareFormComponentExtenderBase<T>` no longer takes `IFieldPermissionService` (or any service) via its constructor — it resolves its dependencies internally. Change custom extenders to a parameterless constructor:
>
> ```csharp
> // Before
> public sealed class RoleAwareMyComponentExtender(IFieldPermissionService svc)
>     : RoleAwareFormComponentExtenderBase<MyComponent>(svc);
>
> // After
> public sealed class RoleAwareMyComponentExtender()
>     : RoleAwareFormComponentExtenderBase<MyComponent>;
> ```
>
> Registration via `[assembly: FormComponentExtender(...)]` is unchanged.

**Why is this needed?** Xperience by Kentico matches extenders by exact form component type — there is no inheritance-based matching. A `FormComponentExtender<TextInputComponent>` will only run for `TextInputComponent`, not for any subclass or other component type.

You can register multiple custom extenders in the same file:

```csharp
[assembly: FormComponentExtender(typeof(RoleAwareCustomDropdownExtender))]
[assembly: FormComponentExtender(typeof(RoleAwareColorPickerExtender))]

namespace MyProject;

public sealed class RoleAwareCustomDropdownExtender()
    : RoleAwareFormComponentExtenderBase<CustomDropdownComponent>;

public sealed class RoleAwareColorPickerExtender()
    : RoleAwareFormComponentExtenderBase<ColorPickerComponent>;
```

## Key Types

| Type | Namespace | Description |
|------|-----------|-------------|
| `IFieldPermissionService` | `XperienceCommunity.FieldPermissions.Services` | Service for checking field restrictions |
| `FieldRestrictionResult` | `XperienceCommunity.FieldPermissions.Services` | Result of a permission check (mode + message + show-in-create-mode) |
| `RoleAwareFormComponentExtenderBase<T>` | `XperienceCommunity.FieldPermissions.Extenders` | Base class for custom component extenders |
| `FieldPermissionInfo` | `XperienceCommunity.FieldPermissions.Classes` | Info object representing a stored permission |
| `FieldPermissionMode` | `XperienceCommunity.FieldPermissions` | Enum: `Disable` (0) or `Hide` (1) |

## Database Structure

The `XperienceCommunity_FieldPermission` table is created automatically on first application start:

| Column | Type | Description |
|--------|------|-------------|
| `FieldPermissionID` | int | Primary key |
| `FieldPermissionGuid` | uniqueidentifier | Unique identifier |
| `FieldPermissionContentTypeID` | int | FK to `CMS_Class.ClassID` |
| `FieldPermissionFieldGuid` | uniqueidentifier | The field's GUID from the content type form definition |
| `FieldPermissionRoleMode` | nvarchar(10) | `allow` (only listed roles may edit) or `disallow` (listed roles may not edit) |
| `FieldPermissionAllowedRoles` | nvarchar(max) | JSON array of role IDs, e.g. `[1,2,3]` |
| `FieldPermissionMode` | int | 0 = Disable, 1 = Hide |
| `FieldPermissionInactiveMessage` | nvarchar(max) | Optional message shown when field is disabled |
| `FieldPermissionShowInCreateMode` | bit | When `true`, restrictions are bypassed while creating a new item |

## Troubleshooting

### Field Permissions Tab Not Appearing

- Ensure `builder.Services.AddFieldPermissions()` is called in `Program.cs`
- Rebuild and restart the application — the module installer runs on first startup

### Permissions Not Being Enforced

- Verify the permission is saved for the correct content type and field
- Check that the user is **not** an Administrator (administrators bypass all restrictions)
- The permission cache refreshes on changes made through the admin UI. If you modify the database directly, restart the application

### Custom Component Not Affected

- Ensure you registered a `FormComponentExtender` for your exact component type
- Verify the `[assembly: FormComponentExtender(...)]` attribute is present
- Check that the component type in your extender matches the type used in the form definition
