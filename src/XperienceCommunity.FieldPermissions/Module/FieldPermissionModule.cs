using CMS.Base;
using CMS.Core;

using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.FieldPermissions.Module;

[assembly: CMS.RegisterModule(typeof(FieldPermissionModule))]

namespace XperienceCommunity.FieldPermissions.Module;

internal class FieldPermissionModule() : CMS.DataEngine.Module(nameof(FieldPermissionModule))
{
    private IFieldPermissionModuleInstaller? installer;

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetService<IFieldPermissionModuleInstaller>();

        if (installer != null)
        {
            ApplicationEvents.Initialized.Execute += InitializeModule;
        }
    }

    private void InitializeModule(object? sender, EventArgs e) => installer?.Install();
}
