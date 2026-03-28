using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Crm;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[XafDisplayName("Oferta de Venta")]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class OfertaVenta(Session session) : DocumentoVenta(session)
{
    private Oportunidad? _oportunidad;
    private EstadoOferta _estadoOferta;

    [Association("Oportunidad-OfertasVenta")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _oportunidad;
        set
        {
            if (!SetPropertyValue(nameof(Oportunidad), ref _oportunidad, value) || IsLoading || IsSaving) return;
            if (value != null)
            {
                if (value.Cliente != null) Cliente = value.Cliente;
                if (value.EquipoVenta != null) EquipoVenta = value.EquipoVenta;
                if (value.Vendedor != null) Vendedor = value.Vendedor;
            }
        }
    }

    [XafDisplayName("Estado")]
    [ModelDefault("AllowEdit", "False")]
    public EstadoOferta EstadoOferta
    {
        get => _estadoOferta;
        set => SetPropertyValue(nameof(EstadoOferta), ref _estadoOferta, value);
    }

    [XafDisplayName("Borrador")]
    public bool Borrador => EstadoOferta == EstadoOferta.Borrador;

    [XafDisplayName("Confirmado")]
    public bool Confirmado => EstadoOferta == EstadoOferta.Aceptada;

    [XafDisplayName("Emitido")]
    public bool Emitido => EstadoOferta == EstadoOferta.Enviada;

    [XafDisplayName("Impreso")]
    public bool Impreso => false;

    [XafDisplayName("Anulado")]
    public bool Anulado => EstadoOferta == EstadoOferta.Rechazada || EstadoOferta == EstadoOferta.Anulada;

    [XafDisplayName("Bloqueado")]
    public bool Bloqueado => false;

    [XafDisplayName("Sincronizado")]
    public bool Sincronizado => false;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstadoOferta = EstadoOferta.Borrador;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoOfertasVentaPorDefecto;
    }
}