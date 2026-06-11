# Xperience Community: Field Permissions

[![NuGet](https://img.shields.io/nuget/v/XperienceCommunity.FieldPermissions.svg)](https://www.nuget.org/packages/XperienceCommunity.FieldPermissions)
[![Downloads](https://img.shields.io/nuget/dt/XperienceCommunity.FieldPermissions?color=cc9900)](https://www.nuget.org/packages/XperienceCommunity.FieldPermissions)
[![License](https://img.shields.io/badge/License-MIT-brightgreen?style=flat)](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/blob/main/LICENSE.md)
[![CI](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/actions/workflows/ci.yml/badge.svg)](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/actions/workflows/ci.yml)
[![GitHub stars](https://img.shields.io/github/stars/roeldeb/xperiencecommunity-fieldpermissions?style=flat&label=stars&logo=github)](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/stargazers)

Role-based field-level access control for Xperience by Kentico admin form editors. Restrict editing of individual fields per content type based on user roles.

![Field Permissions Demo](https://raw.githubusercontent.com/roeldeb/xperiencecommunity-fieldpermissions/main/images/field-permissions-demo.png)

![Field Permissions Demo](https://raw.githubusercontent.com/roeldeb/xperiencecommunity-fieldpermissions/main/images/field-permissions-demo-2.png)

> **⚠️ Internal API Warning:** This library depends on Kentico internal APIs that are not part of the public API surface and may change between Xperience by Kentico versions without notice — `ReusableFieldSchemaUtils` (`CMS.ContentEngine.Internal`, used to resolve fields contributed by reusable field schemas), and `ContentItemInfo`/`WebPageItemInfo` (`CMS.ContentEngine.Internal` / `CMS.Websites.Internal`, used to resolve the edited content type). It also relies on Kentico admin URL/request conventions to detect create mode and the current content type. Upgrading to a newer version of Xperience by Kentico may temporarily break this library until a compatible update is released.

## Description

This module provides role-based field-level permissions that can be configured per content type through the Kentico admin interface:

- **Disable fields** — Fields are shown as read-only with an optional message for users without the required role
- **Hide fields** — Fields are completely hidden from users without the required role
- **Show in create mode** — Optionally bypass a field's restrictions while creating a new item, so a restricted (e.g. required) field can still be filled in; restrictions still apply when editing
- **Per content type** — Configure field permissions through a "Field permissions" tab on each content type (fields shared via reusable field schemas are resolved to the correct content type)

Field permissions are enforced using `FormComponentExtender<T>`, the official Xperience by Kentico extension point for modifying form component behavior.

## Example usecase

**Meet Maria, content lead at a busy marketing team running Xperience by Kentico.**

- 😬 **The problem:** Her *Article* content type has a "Legal disclaimer" field that Legal carefully signed off on. But any junior author can open the article and edit it — and they do, by accident.
- 🎫 **Before:** Maria's only options were "trust everyone" or "open a developer ticket" every time a field needed locking down. Neither scaled.
- ✨ **Enter Field Permissions:** She opens the *Article* content type, clicks the **Field permissions** tab, and in a few clicks says: *only the Legal role can edit "Legal disclaimer."*
- 🔒 **For everyone else:** the field is now **read-only** with a friendly note — *"Contact Legal to change this."* No code, no deployment.
- 🙈 **One step further:** The sensitive "Internal cost" field? She sets it to **Hide** — authors don't even see it exists.
- 🆕 **But creation still works:** "Internal cost" is required when an article is first created. She ticks **Show in create mode**, so authors can fill it in once at creation — but can't touch it afterwards.
- 🎯 **The result:** Legal keeps control, authors stay productive, and Maria never files another "please lock this field" ticket.

> **Field Permissions** turns "we need a developer for that" into "I'll do it myself in 30 seconds" — role-based, per-field, right inside the Kentico admin.

## Requirements

### Library Version Matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 31.3.0         | 1.0.0           |

### Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

## Package Installation

Add the package to your application using the .NET CLI

```powershell
dotnet add package XperienceCommunity.FieldPermissions
```

## Quick Start

### 1. Register in Program.cs

```csharp
using XperienceCommunity.FieldPermissions;

builder.Services.AddFieldPermissions();
```

### 2. Configure Field Permissions in the Admin

Navigate to **Content types** → select a content type → **Field permissions** tab.

From there you can:
- Select a field from the content type
- Choose the **role mode** — *Allow* (only the selected roles can edit the field) or *Disallow* (the selected roles cannot edit the field; everyone else can)
- Select the roles the rule applies to
- Set the restriction mode (Disable or Hide)
- Optionally set a message shown to restricted users
- Optionally enable **Show field in create mode** — the field's restrictions are bypassed while creating a new item (they still apply when editing)

#### Multiple Rules per Field

You can create multiple permission rules for the same field. Each rule is evaluated independently using **OR** logic:

- **If the user passes any rule** → the field is **unrestricted** (fully editable)
- **If the user fails all rules** → the **most restrictive** mode is applied (Hide > Disable)

Global administrators always bypass all permission checks.

**How each rule is evaluated:**

| Role Mode | User matches role? | Result |
|-----------|-------------------|--------|
| Allow     | Yes               | Passes (unrestricted) |
| Allow     | No                | Fails (restricted) |
| Disallow  | Yes               | Fails (restricted) |
| Disallow  | No                | Passes (unrestricted) |

**Example** — two rules on the same field:

| Rule | Role Mode | Roles   | Mode    |
|------|-----------|---------|---------|
| A    | Allow     | Editor  | Disable |
| B    | Allow     | Author  | Hide    |

- An **Editor** passes rule A → **unrestricted**
- An **Author** passes rule B → **unrestricted**
- A user who is **neither** fails both → **Hidden** (most restrictive mode wins)

### 3. Extending Custom Form Components

The package includes built-in extenders for all standard Xperience form components. If your project uses custom form components (e.g., from a shared library), you can add support with a one-liner:

```csharp
using XperienceCommunity.FieldPermissions.Extenders;

using Kentico.Xperience.Admin.Base.Forms;

[assembly: FormComponentExtender(typeof(RoleAwareCustomDropdownExtender))]

namespace MyProject;

// One-liner: inherit the base — that's it.
// XbyK matches extenders by exact component type, so each custom
// component needs its own extender class.
public sealed class RoleAwareCustomDropdownExtender()
    : RoleAwareFormComponentExtenderBase<CustomDropdownComponent>;
```

> **⚠️ Breaking change when upgrading from 1.0.0 — custom form component extenders.** `RoleAwareFormComponentExtenderBase<T>` no longer takes `IFieldPermissionService` (or any service) via its constructor; it now resolves its dependencies internally. Update custom extenders to a parameterless constructor:
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

## Full Instructions

View the [Usage Guide](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/blob/main/docs/Usage-Guide.md) for complete documentation including:

- How field permission enforcement works
- Extending custom form components
- Key types reference
- Database structure
- Troubleshooting

## Contributing

Feel free to submit issues or pull requests to the repository, this is a community package and everyone is welcome to support.

## License

Distributed under the MIT License. See [`LICENSE.md`](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/blob/main/LICENSE.md) for more information.
