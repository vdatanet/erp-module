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
        // En el Host (TenantId == null), el DatabaseUpdater de XAF ya ha creado el esquema antes de llegar aquí.
        // El problema es que el ObjectSpaceProvider del Host contiene todos los tipos de negocio.
        
        base.UpdateDatabaseAfterUpdateSchema();

        // Si estamos en un TENANT, TenantId NO será null.
        // Si estamos en el HOST, TenantId SERÁ null.
        
        var dataSeedService = ObjectSpace.ServiceProvider?.GetService<IDataSeedService>();
        if (dataSeedService != null)
        {
            // Forzar que el contexto sea explícito según el TenantId del Updater
            dataSeedService.Seed(ObjectSpace, TenantName, TenantId);
        }

        ObjectSpace.CommitChanges();
    }

    public override void UpdateDatabaseBeforeUpdateSchema()
    {
        base.UpdateDatabaseBeforeUpdateSchema();
    }
}