using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Crm;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class Presupuesto(Session session) : DocumentoVenta(session)
{
    private Oportunidad _oportunidad;

    [Association("Oportunidad-Presupuestos")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad Oportunidad
    {
        get => _oportunidad;
        set
        {
            var oldOportunidad = _oportunidad;
            if (!SetPropertyValue(nameof(Oportunidad), ref _oportunidad, value) || IsLoading || IsSaving) return;
            if (value != null && value.Cliente != null) Cliente = value.Cliente;

            oldOportunidad?.ActualizarSumaPresupuestos(true);
            _oportunidad?.ActualizarSumaPresupuestos(true);
        }
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (IsLoading || IsSaving || IsDeleted) return;
        if (propertyName == nameof(BaseImponible)) Oportunidad?.ActualizarSumaPresupuestos(true);
    }
}