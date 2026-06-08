using Kentico.Web.Mvc;
using Kentico.Xperience.Admin.Base.Forms;

using XperienceCommunity.FieldPermissions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKentico();

// Register field permissions services
builder.Services.AddFieldPermissions();

var app = builder.Build();

app.InitKentico();

app.UseKentico();

await app.RunAsync();

// Example: Extend a custom form component with field permissions.
// XbyK matches extenders by exact type, so each custom component
// needs its own one-liner class:
//
// using XperienceCommunity.FieldPermissions.Extenders;
// using XperienceCommunity.FieldPermissions.Services;
//
// [assembly: FormComponentExtender(typeof(RoleAwareCustomDropdownExtender))]
//
// public sealed class RoleAwareCustomDropdownExtender(IFieldPermissionService svc)
//     : RoleAwareFormComponentExtenderBase<CustomDropdownComponent>(svc);

