# Xperience Community: Field Permissions

[![NuGet](https://img.shields.io/nuget/v/XperienceCommunity.FieldPermissions.svg)](https://www.nuget.org/packages/XperienceCommunity.FieldPermissions)
[![Downloads](https://img.shields.io/nuget/dt/XperienceCommunity.FieldPermissions?color=cc9900)](https://www.nuget.org/packages/XperienceCommunity.FieldPermissions)
[![License](https://img.shields.io/badge/License-MIT-brightgreen?style=flat)](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/blob/main/LICENSE.md)
[![CI](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/actions/workflows/ci.yml/badge.svg)](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/actions/workflows/ci.yml)
[![GitHub stars](https://img.shields.io/github/stars/roeldeb/xperiencecommunity-fieldpermissions?style=flat&label=stars&logo=github)](https://github.com/roeldeb/xperiencecommunity-fieldpermissions/stargazers)

Role-based field-level access control for Xperience by Kentico admin form editors. Restrict editing of individual fields per content type based on user roles.

## Description

This module provides role-based field-level permissions that can be configured per content type through the Kentico admin interface:

- **Disable fields** — Fields are shown as read-only with an optional message for users without the required role
- **Hide fields** — Fields are completely hidden from users without the required role
- **Per content type** — Configure field permissions through a "Field permissions" tab on each content type

Field permissions are enforced using `FormComponentExtender<T>`, the official Xperience by Kentico extension point for modifying form component behavior.

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
- Choose which roles are allowed to edit that field
- Set the restriction mode (Disable or Hide)
- Optionally set a message shown to restricted users

### 3. Extending Custom Form Components

The package includes built-in extenders for all standard Xperience form components. If your project uses custom form components (e.g., from a shared library), you can add support with a one-liner:

```csharp
using XperienceCommunity.FieldPermissions.Extenders;
using XperienceCommunity.FieldPermissions.Services;

using Kentico.Xperience.Admin.Base.Forms;

[assembly: FormComponentExtender(typeof(RoleAwareCustomDropdownExtender))]

namespace MyProject;

// One-liner: inherit the base and pass the service through.
// XbyK matches extenders by exact component type, so each custom
// component needs its own extender class.
public sealed class RoleAwareCustomDropdownExtender(IFieldPermissionService svc)
    : RoleAwareFormComponentExtenderBase<CustomDropdownComponent>(svc);
```

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
