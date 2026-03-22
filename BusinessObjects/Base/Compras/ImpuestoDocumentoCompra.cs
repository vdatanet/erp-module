using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Base.Compras;

[ImageName("RowTotalsPosition")]
[DefaultProperty(nameof(Secuencia))]
public class ImpuestoDocumentoCompra(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private CuentaContable? _cuenta;
    private DocumentoCompra? _documentoCompra;
    private bool _esRetencion;
    private decimal _importeImpuestos;
    private int _secuencia;
    private decimal _tipo;
    private TipoImpuesto? _tipoImpuesto;

    [Association("DocumentoCompra-Impuestos")]
    [XafDisplayName("Documento Compra")]
    public DocumentoCompra? DocumentoCompra
    {
        get => _documentoCompra;
        set => SetPropertyValue(nameof(DocumentoCompra), ref _documentoCompra, value);
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

    [XafDisplayName("CuentaContable")]
    public CuentaContable? CuentaContable
    {
        get => _cuenta;
        set => SetPropertyValue(nameof(CuentaContable), ref _cuenta, value);
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
            CuentaContable = null;
            Tipo = 0;
            EsRetencion = false;
            return;
        }

        Secuencia = TipoImpuesto.Secuencia;
        CuentaContable = TipoImpuesto.CuentaContable;
        Tipo = TipoImpuesto.Tipo;
        EsRetencion = TipoImpuesto.EsRetencion;
    }

    private void CalcularImporteImpuesto()
    {
        ImporteImpuestos = AmountCalculator.GetTaxAmount(BaseImponible, Tipo, EsRetencion);
    }
}