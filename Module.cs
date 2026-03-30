using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Chart;
using DevExpress.ExpressApp.CloneObject;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Dashboards;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Notifications;
using DevExpress.ExpressApp.Office;
using DevExpress.ExpressApp.PivotGrid;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.Scheduler;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.TreeListEditors;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Validation;
using DevExpress.ExpressApp.ViewVariantsModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.Services.Configuraciones;
using erp.Module.Models.Configuraciones;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.ObjectBinding;
using Updater = erp.Module.DatabaseUpdate.Updater;

namespace erp.Module;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
public sealed class erpModule : ModuleBase
{
    public erpModule()
    {
        //
        // erpModule
        //
        AdditionalExportedTypes.Add(typeof(ModelDifference));
        AdditionalExportedTypes.Add(typeof(ModelDifferenceAspect));
        AdditionalExportedTypes.Add(typeof(BaseObject));
        AdditionalExportedTypes.Add(typeof(FileData));
        AdditionalExportedTypes.Add(typeof(FileAttachmentBase));
        AdditionalExportedTypes.Add(typeof(Event));
        AdditionalExportedTypes.Add(typeof(Resource));
        AdditionalExportedTypes.Add(typeof(HCategory));
        //AdditionalExportedTypes.Add(typeof(ImportarClientesParameters));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.UserToken));
        AdditionalExportedTypes.Add(typeof(Pais));
        AdditionalExportedTypes.Add(typeof(Provincia));
        AdditionalExportedTypes.Add(typeof(Poblacion));
        AdditionalExportedTypes.Add(typeof(VeriFactuAudit));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant));
        AdditionalExportedTypes.Add(typeof(ApplicationUser));
        AdditionalExportedTypes.Add(typeof(ApplicationRole));
        AdditionalExportedTypes.Add(typeof(ApplicationUserLoginInfo));
        RequiredModuleTypes.Add(typeof(SystemModule));
        RequiredModuleTypes.Add(typeof(SecurityModule));
        RequiredModuleTypes.Add(typeof(ChartModule));
        RequiredModuleTypes.Add(typeof(CloneObjectModule));
        RequiredModuleTypes.Add(typeof(ConditionalAppearanceModule));
        RequiredModuleTypes.Add(typeof(DashboardsModule));
        RequiredModuleTypes.Add(typeof(NotificationsModule));
        RequiredModuleTypes.Add(typeof(OfficeModule));
        RequiredModuleTypes.Add(typeof(PivotGridModule));
        RequiredModuleTypes.Add(typeof(ReportsModuleV2));
        RequiredModuleTypes.Add(typeof(SchedulerModuleBase));
        RequiredModuleTypes.Add(typeof(TreeListEditorsModuleBase));
        RequiredModuleTypes.Add(typeof(ValidationModule));
        RequiredModuleTypes.Add(typeof(ViewVariantsModule));
    }

    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
    {
        var updater = new Updater(objectSpace, versionFromDB);

        try
        {
            var tenantProvider = objectSpace.ServiceProvider?.GetService<ITenantProvider>();
            if (tenantProvider != null)
            {
                updater.TenantIdOverride = tenantProvider.TenantId;
                updater.TenantNameOverride = tenantProvider.TenantName;
            }
        }
        catch (Exception)
        {
        }

        return new[] { updater };
    }

    public override void Setup(XafApplication application)
    {
        base.Setup(application);
        application.SetupComplete += Application_SetupComplete;
    }

    private void Application_SetupComplete(object? sender, EventArgs e)
    {
        var application = (XafApplication)sender!;
        var reportsModule = application.Modules.FindModule<ReportsModuleV2>();
        if (reportsModule != null)
        {
            reportsModule.ReportsDataSourceHelper.BeforeShowPreview += ReportsDataSourceHelper_BeforeShowPreview;
        }
    }

    private void ReportsDataSourceHelper_BeforeShowPreview(object? sender, BeforeShowPreviewEventArgs e)
    {
        // En XAF, el ObjectSpace suele estar disponible a través del DataSource del reporte o el sender
        IObjectSpace? objectSpace = null;
        
        // Intentamos obtener el ObjectSpace del sender mediante reflexión
        var prop = sender?.GetType().GetProperty("ObjectSpace");
        if (prop != null)
        {
            objectSpace = prop.GetValue(sender) as IObjectSpace;
        }

        // Si es nulo y estamos en un reporte de XAF, intentamos obtenerlo de Application si está disponible
        if (objectSpace == null && Application != null)
        {
            objectSpace = Application.CreateObjectSpace(typeof(ReportDataV2));
        }

        if (objectSpace != null)
        {
            InjectCompanyInfo(e.Report, objectSpace);
        }
    }

    private void InjectCompanyInfo(XtraReport report, IObjectSpace objectSpace)
    {
        var empresaProvider = objectSpace.ServiceProvider?.GetService<IInformacionEmpresaProvider>();
        if (empresaProvider == null) return;

        var companyDto = empresaProvider.GetInformacionEmpresaDto(objectSpace);

        // Opción 1: Parámetros con prefijo Empresa_
        var properties = typeof(InformacionEmpresaDto).GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(companyDto);
            
            // Inyectamos con prefijo Empresa_ (Ej: Empresa_Nombre)
            SetReportParameter(report, $"Empresa_{prop.Name}", value);
            
            // Inyectamos también con prefijo Empresa (Ej: EmpresaNombre) por si acaso
            SetReportParameter(report, $"Empresa{prop.Name}", value);
        }

        // Opción 2: ObjectDataSource configurado con InformacionEmpresaDto
        foreach (var dataSource in report.ComponentStorage.OfType<ObjectDataSource>())
        {
            if (dataSource.DataSourceType == typeof(InformacionEmpresaDto))
            {
                dataSource.DataSource = companyDto;
            }
        }
    }

    private void SetReportParameter(XtraReport report, string paramName, object? value)
    {
        var parameter = report.Parameters[paramName];
        if (parameter == null)
        {
            parameter = new DevExpress.XtraReports.Parameters.Parameter
            {
                Name = paramName,
                Visible = false,
                Value = value
            };
            report.Parameters.Add(parameter);
        }
        else
        {
            parameter.Value = value;
        }

        // Si el valor es una cadena y es nula o vacía, nos aseguramos de que el parámetro tenga al menos un valor vacío
        // para evitar que el reporte pida el valor al usuario si está marcado como no nulo.
        if (parameter.Type == typeof(string) && parameter.Value == null)
        {
            parameter.Value = string.Empty;
        }
    }

    public override void CustomizeTypesInfo(ITypesInfo typesInfo)
    {
        base.CustomizeTypesInfo(typesInfo);
        CalculatedPersistentAliasHelper.CustomizeTypesInfo(typesInfo);
    }
}