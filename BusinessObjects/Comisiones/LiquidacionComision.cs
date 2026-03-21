using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using System.ComponentModel;
using System.Linq;

namespace erp.Module.BusinessObjects.Comisiones;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[XafDisplayName("Liquidación de Comisiones")]
[ImageName("BO_List")]
public class LiquidacionComision(Session session) : EntidadBase(session)
{
    private Contacto? _vendedor;
    private int _mes;
    private int _anio;
    private DateTime _fechaLiquidacion;
    private string? _notas;

    [XafDisplayName("Vendedor")]
    [DataSourceCriteria("EsVendedor = true")]
    public Contacto? Vendedor
    {
        get => _vendedor;
        set => SetPropertyValue(nameof(Vendedor), ref _vendedor, value);
    }

    [XafDisplayName("Mes")]
    public int Mes
    {
        get => _mes;
        set => SetPropertyValue(nameof(Mes), ref _mes, value);
    }

    [XafDisplayName("Año")]
    public int Anio
    {
        get => _anio;
        set => SetPropertyValue(nameof(Anio), ref _anio, value);
    }

    [XafDisplayName("Fecha Liquidación")]
    public DateTime FechaLiquidacion
    {
        get => _fechaLiquidacion;
        set => SetPropertyValue(nameof(FechaLiquidacion), ref _fechaLiquidacion, value);
    }

    [XafDisplayName("Notas")]
    [Size(SizeAttribute.Unlimited)]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Comisiones")]
    [Association("Liquidacion-Comisiones")]
    public XPCollection<Comision> Comisiones => GetCollection<Comision>();

    [XafDisplayName("Total Liquidado")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    public decimal TotalLiquidado => Comisiones.ToList().Sum(c => c.Importe);

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaLiquidacion = DateTime.Today;
        Mes = DateTime.Today.Month;
        Anio = DateTime.Today.Year;
    }
}
