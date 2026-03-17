using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[XafDisplayName("Simulación")]
[ImageName("BO_Scheduler")]
public class Simulacion(Session session) : EventoBase(session)
{
    private Alquiler _alquiler;

    [Association("Alquiler-Simulaciones")]
    [XafDisplayName("Alquiler")]
    public Alquiler Alquiler
    {
        get => _alquiler;
        set => SetPropertyValue(nameof(Alquiler), ref _alquiler, value);
    }
}
