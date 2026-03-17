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
using erp.Module.BusinessObjects;
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
        ModuleUpdater updater = new Updater(objectSpace, versionFromDB);
        return new[] { updater };
    }

    public override void Setup(XafApplication application)
    {
        base.Setup(application);
        // Manage various aspects of the application UI and behavior at the module level.
    }

    public override void CustomizeTypesInfo(ITypesInfo typesInfo)
    {
        base.CustomizeTypesInfo(typesInfo);
        CalculatedPersistentAliasHelper.CustomizeTypesInfo(typesInfo);

        var reservaInfo = typesInfo.FindTypeInfo(typeof(BusinessObjects.Alquileres.Reserva));
        if (reservaInfo != null)
        {
            var startOnInfo = reservaInfo.FindMember("StartOn");
            startOnInfo?.AddAttribute(new DevExpress.ExpressApp.Model.ModelDefaultAttribute("DisplayFormat", "{0:d}"));
            startOnInfo?.AddAttribute(new DevExpress.ExpressApp.Model.ModelDefaultAttribute("EditMask", "d"));

            var endOnInfo = reservaInfo.FindMember("EndOn");
            endOnInfo?.AddAttribute(new DevExpress.ExpressApp.Model.ModelDefaultAttribute("DisplayFormat", "{0:d}"));
            endOnInfo?.AddAttribute(new DevExpress.ExpressApp.Model.ModelDefaultAttribute("EditMask", "d"));
        }
    }
}