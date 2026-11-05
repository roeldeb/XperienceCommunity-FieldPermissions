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

// Custom form components need their own extender class, because Xperience
// matches extenders by exact component type. See "Extending Custom Form Components"
// in docs/Usage-Guide.md for the one-liner subclass of
// RoleAwareFormComponentExtenderBase<T> and its registration.
