using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.Helpers.Comun;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Base.Ventas;

[ImageName("RowTotalsPosition")]
[DefaultProperty(nameof(Secuencia))]
public class ImpuestoDocumentoVenta(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private CausaExencion? _causaExencion;
    private Cuenta? _cuenta;
    private DocumentoVenta? _documentoVenta;
    private bool _esRetencion;
    private decimal _importeImpuestos;
    private Impuesto? _impuesto;
    private ClaveRegimen? _regimenFiscal;
    private int _secuencia;
    private decimal _tipo;
    private TipoImpuesto? _tipoImpuesto;
    private CalificacionOperacion? _tipoOperacion;

    [Association("DocumentoVenta-Impuestos")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [XafDisplayName("Secuencia")]
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [ImmediatePostData]
    [XafDisplayName("Tipo Impuesto")]
    public TipoImpuesto? TipoImpuesto
    {
        get => _tipoImpuesto;
        set
        {
            var modified = SetPropertyValue(nameof(TipoImpuesto), ref _tipoImpuesto, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            AplicarInstantaneaImpuesto();
        }
    }

    [XafDisplayName("Cuenta")]
    public Cuenta? Cuenta
    {
        get => _cuenta;
        set => SetPropertyValue(nameof(Cuenta), ref _cuenta, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ImmediatePostData]
    [XafDisplayName("Tipo %")]
    public decimal Tipo
    {
        get => _tipo;
        set
        {
            var modified = SetPropertyValue(nameof(Tipo), ref _tipo, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            CalcularImporteImpuesto();
        }
    }

    [ImmediatePostData]
    [XafDisplayName("Es Retención")]
    public bool EsRetencion
    {
        get => _esRetencion;
        set
        {
            var modified = SetPropertyValue(nameof(EsRetencion), ref _esRetencion, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            CalcularImporteImpuesto();
        }
    }

    [XafDisplayName("Impuesto VeriFactu")]
    public Impuesto? Impuesto
    {
        get => _impuesto;
        set => SetPropertyValue(nameof(Impuesto), ref _impuesto, value);
    }

    [XafDisplayName("Posición Fiscal")]
    public ClaveRegimen? RegimenFiscal
    {
        get => _regimenFiscal;
        set => SetPropertyValue(nameof(RegimenFiscal), ref _regimenFiscal, value);
    }

    [XafDisplayName("Tipo Operación")]
    public CalificacionOperacion? TipoOperacion
    {
        get => _tipoOperacion;
        set => SetPropertyValue(nameof(TipoOperacion), ref _tipoOperacion, value);
    }

    [XafDisplayName("Causa Exención")]
    public CausaExencion? CausaExencion
    {
        get => _causaExencion;
        set => SetPropertyValue(nameof(CausaExencion), ref _causaExencion, value);
    }

    [ImmediatePostData]
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set
        {
            var modified = SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            CalcularImporteImpuesto();
        }
    }

    [XafDisplayName("Impuesto")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    private void AplicarInstantaneaImpuesto()
    {
        if (TipoImpuesto is null)
        {
            Secuencia = 0;
            Cuenta = null;
            Tipo = 0;
            EsRetencion = false;
            Impuesto = null;
            RegimenFiscal = null;
            TipoOperacion = null;
            CausaExencion = null;
            return;
        }

        Secuencia = TipoImpuesto.Secuencia;
        Cuenta = TipoImpuesto.Cuenta;
        Tipo = TipoImpuesto.Tipo;
        EsRetencion = TipoImpuesto.EsRetencion;
        Impuesto = TipoImpuesto.Impuesto;
        RegimenFiscal = TipoImpuesto.RegimenFiscal;
        TipoOperacion = TipoImpuesto.TipoOperacion;
        CausaExencion = TipoImpuesto.CausaExencion;
    }

    private void CalcularImporteImpuesto()
    {
        ImporteImpuestos = AmountCalculator.GetTaxAmount(BaseImponible, Tipo, EsRetencion);
    }
}