using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[XafDisplayName("Albarán de Venta")]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class AlbaranVenta(Session session) : DocumentoVenta(session)
{
    private EstadoAlbaran _estadoAlbaran;

    [XafDisplayName("Estado")]
    [ModelDefault("AllowEdit", "False")]
    public EstadoAlbaran EstadoAlbaran
    {
        get => _estadoAlbaran;
        set => SetPropertyValue(nameof(EstadoAlbaran), ref _estadoAlbaran, value);
    }

    [XafDisplayName("Borrador")]
    public bool Borrador => EstadoAlbaran == EstadoAlbaran.Borrador;

    [XafDisplayName("Confirmado")]
    public bool Confirmado => EstadoAlbaran >= EstadoAlbaran.Emitido && EstadoAlbaran != EstadoAlbaran.Anulado;

    [XafDisplayName("Emitido")]
    public bool Emitido => EstadoAlbaran >= EstadoAlbaran.Enviado && EstadoAlbaran != EstadoAlbaran.Anulado;

    [XafDisplayName("Impreso")]
    public bool Impreso => false;

    [XafDisplayName("Anulado")]
    public bool Anulado => EstadoAlbaran == EstadoAlbaran.Anulado;

    [XafDisplayName("Bloqueado")]
    public bool Bloqueado => false;

    [XafDisplayName("Sincronizado")]
    public bool Sincronizado => false;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstadoAlbaran = EstadoAlbaran.Borrador;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoAlbaranesVentaPorDefecto;
    }
}