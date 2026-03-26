using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Updating;
using erp.Module.BusinessObjects;
using erp.Module.Services.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater
{
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion)
    {
    }

    public Guid? TenantIdOverride { get; set; }
    public string? TenantNameOverride { get; set; }

    private Guid? TenantId => TenantIdOverride ?? ObjectSpace.ServiceProvider?.GetService<ITenantProvider>()?.TenantId;

    private string? TenantName => TenantNameOverride ?? ObjectSpace.ServiceProvider?.GetService<ITenantProvider>()?.TenantName;

    public override void UpdateDatabaseAfterUpdateSchema()
    {
        base.UpdateDatabaseAfterUpdateSchema();

// TODO: quitar cuando la aplicación este en producción
#if DEBUG
        if (ObjectSpace.ServiceProvider != null)
        {
            var dataSeedService = ObjectSpace.ServiceProvider.GetService<IDataSeedService>();
            if (dataSeedService != null)
            {
                dataSeedService.Seed(ObjectSpace, TenantName, TenantId);
            }
            else
            {
                // Fallback for cases where DataSeedService might not be registered or available in the current context
                var securitySetup = new SecuritySetupService(ObjectSpace);
                securitySetup.CreateRolesAndUsers(TenantName, onlyAdmin: true);
            }
        }
#endif

        ObjectSpace.CommitChanges();
    }

    public override void UpdateDatabaseBeforeUpdateSchema()
    {
        base.UpdateDatabaseBeforeUpdateSchema();
        //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
        //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
        //}
    }
}