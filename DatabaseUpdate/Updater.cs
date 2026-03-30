using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Updating;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Base.Facturacion;
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
        var seedService = ObjectSpace.ServiceProvider?.GetService<IDataSeedService>();
        if (seedService != null)
        {
            seedService.Seed(ObjectSpace, TenantName, TenantId);
        }

        ObjectSpace.CommitChanges();
    }

    public override void UpdateDatabaseBeforeUpdateSchema()
    {
        base.UpdateDatabaseBeforeUpdateSchema();

        if (ObjectSpace is DevExpress.ExpressApp.Xpo.XPObjectSpace xpObjectSpace)
        {
            var session = xpObjectSpace.Session;
            try
            {
                session.UpdateSchema(typeof(FacturaBase));
            }
            catch { /* Silencioso */ }
        }
    }
}