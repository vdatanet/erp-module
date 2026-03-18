using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.BaseImpl.MultiTenancy;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Configuraciones;
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

    private Guid? TenantId => ObjectSpace.ServiceProvider.GetRequiredService<ITenantProvider>().TenantId;

    private string TenantName => ObjectSpace.ServiceProvider.GetRequiredService<ITenantProvider>().TenantName;

    public override void UpdateDatabaseAfterUpdateSchema()
    {
        base.UpdateDatabaseAfterUpdateSchema();

        if (!ObjectSpace.CanInstantiate(typeof(ApplicationUser))) return;

        new TenantSetupService(ObjectSpace).CreateInitialTenants(TenantName);

        new SecuritySetupService(ObjectSpace).CreateRolesAndUsers(TenantName);

        ObjectSpace.CommitChanges(); //This line persists created object(s).
        if (TenantId != null)
        {
            var informacionEmpresa = ObjectSpace.FirstOrDefault<InformacionEmpresa>(i => true);
            if (informacionEmpresa == null)
            {
                informacionEmpresa = ObjectSpace.CreateObject<InformacionEmpresa>();
                informacionEmpresa.Nombre = "Empresa por Defecto";
                informacionEmpresa.Nif = "B00000000";
                ObjectSpace.CommitChanges();
            }
        }
    }

    public override void UpdateDatabaseBeforeUpdateSchema()
    {
        base.UpdateDatabaseBeforeUpdateSchema();
        //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
        //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
        //}
    }
}