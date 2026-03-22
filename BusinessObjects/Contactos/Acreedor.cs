using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Vendor")]
public class Acreedor(Session session) : Tercero(session)
{
    public override string GetPrefijoCodigo()
    {
        return "A";
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        CuentaContable = companyInfo.CuentaAcreedoresPorDefecto;
        if (CuentaContable != null && (!CuentaContable.EstaActiva || !CuentaContable.EsAsentable))
        {
            CuentaContable = null;
        }
    }
}
