using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Produccion;

[DefaultClassOptions]
[NavigationItem("Producción")]
[ImageName("BO_Order")]
public class Parte(Session session) : DocumentoVenta(session)
{
    private bool _esFacturable;
    private double _horasTotales;
    private decimal _importeManoObra;
    private decimal _importeMateriales;

    [XafDisplayName("¿Es Facturable?")]
    public bool EsFacturable
    {
        get => _esFacturable;
        set => SetPropertyValue(nameof(EsFacturable), ref _esFacturable, value);
    }

    [XafDisplayName("Horas Totales")]
    public double HorasTotales
    {
        get => _horasTotales;
        set => SetPropertyValue(nameof(HorasTotales), ref _horasTotales, value);
    }

    [XafDisplayName("Importe Mano de Obra")]
    public decimal ImporteManoObra
    {
        get => _importeManoObra;
        set => SetPropertyValue(nameof(ImporteManoObra), ref _importeManoObra, value);
    }

    [XafDisplayName("Importe Materiales")]
    public decimal ImporteMateriales
    {
        get => _importeMateriales;
        set => SetPropertyValue(nameof(ImporteMateriales), ref _importeMateriales, value);
    }

    [Association("Parte-Tiempos")]
    [XafDisplayName("Tiempos")]
    public XPCollection<ParteTrabajoTiempo> Tiempos => GetCollection<ParteTrabajoTiempo>(nameof(Tiempos));

    [Association("Parte-Materiales")]
    [XafDisplayName("Materiales")]
    public XPCollection<ParteTrabajoMaterial> Materiales => GetCollection<ParteTrabajoMaterial>(nameof(Materiales));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Serie ??= companyInfo?.PrefijoParteTrabajoPorDefecto;
        EsFacturable = true;
    }
}