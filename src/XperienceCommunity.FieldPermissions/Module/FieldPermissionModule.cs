using CMS.Base;
using CMS.Core;

using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.FieldPermissions.Module;

[assembly: CMS.RegisterModule(typeof(FieldPermissionModule))]

namespace XperienceCommunity.FieldPermissions.Module;

internal class FieldPermissionModule() : CMS.DataEngine.Module(nameof(FieldPermissionModule))
{
    private IFieldPermissionModuleInstaller? installer;
    private IReadOnlyModeProvider? readOnlyModeProvider;

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetService<IFieldPermissionModuleInstaller>();
        readOnlyModeProvider = services.GetService<IReadOnlyModeProvider>();

        if (installer != null)
        {
            ApplicationEvents.Initialized.Execute += InitializeModule;
        }
    }

    private void InitializeModule(object? sender, EventArgs e)
    {
        // Read-only (zero-downtime) deployments serve a frozen database snapshot where every
        // write throws ReadOnlyModeException. Skip the installation; it runs once the updated
        // instance starts in normal (writable) mode.
        if (readOnlyModeProvider?.IsReadOnly == true)
        {
            return;
        }

        installer?.Install();
    }
}
