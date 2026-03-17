using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Impuestos;

[DefaultClassOptions]
[NavigationItem("Impuestos")]
[ImageName("BO_Category")]
[DefaultProperty(nameof(Nombre))]
public class PosicionFiscal(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _notas;

    [Size(255)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Association("PosicionFiscal-Mapeos")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Mapeos de Impuestos")]
    public XPCollection<MapeoImpuesto> Mapeos => GetCollection<MapeoImpuesto>();

    [Association("PosicionFiscal-MapeosCuenta")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Mapeos de Cuentas")]
    public XPCollection<MapeoCuenta> MapeosCuenta => GetCollection<MapeoCuenta>();
}