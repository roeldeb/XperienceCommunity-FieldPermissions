---
applyTo: "**"
---

# XperienceCommunity.FieldPermissions — Copilot Instructions

## Project Context
This is an open-source NuGet package providing role-based field-level access control for Xperience by Kentico admin form editors.

## Architecture
- **Package**: `XperienceCommunity.FieldPermissions` (src/)
- **Example**: DancingGoat (examples/) — NOT part of the package
- **Solution**: `XperienceCommunity.FieldPermissions.slnx`

## Key Patterns
- Uses `FormComponentExtender<T>` (XbyK's official extension point) for field enforcement
- Matching is by **field GUID** (unique per field per content type, available during `ConfigureComponent()`)
- Info objects follow the XbyK `AbstractInfo<T>` pattern
- Module registration uses `[assembly: RegisterModule]`
- DI registration uses `IServiceCollection.AddFieldPermissions()` extension method

## Naming Conventions
- Namespace: `XperienceCommunity.FieldPermissions`
- DB Object Type: `xperiencecommunity.fieldpermission`
- No references to "TrueLime" — this is a community package

## Reference Projects
- Structure mirrors `XperienceCommunity.ProjectSettings`
- XbyK source at `d:\Projects\Kentico\xperience-by-kentico` for API reference (read-only)
