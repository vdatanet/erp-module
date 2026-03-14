using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Taxes;
using erp.Module.Helpers.Common;
using System.ComponentModel;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Base.Sales;

[ImageName("RowTotalsPosition")]
[DefaultProperty(nameof(Secuencia))]
[NavigationItem("Taxes")]
public class SalesDocumentTax(Session session): BaseEntity(session)
{
    private SalesDocument _documentoVenta;
    private int _secuencia;
    private TaxKind _tipoImpuesto;
    private Account _cuenta;
    private decimal _tipo;
    private bool _esRetencion;
    private Impuesto? _impuesto;
    private ClaveRegimen? _regimenFiscal;
    private CalificacionOperacion? _tipoOperacion;
    private CausaExencion? _causaExencion;
    
    private decimal _baseImponible;
    private decimal _importeImpuestos;
    
    [Association("SalesDocument-Taxes")]
    public SalesDocument DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }
    
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [ImmediatePostData]
    public TaxKind TipoImpuesto
    {
        get => _tipoImpuesto;
        set
        {
            bool modified = SetPropertyValue(nameof(TipoImpuesto), ref _tipoImpuesto, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            AplicarInstantaneaImpuesto();
        }
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

    public Account Cuenta
    {
        get => _cuenta;
        set => SetPropertyValue(nameof(Cuenta), ref _cuenta, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ImmediatePostData]
    public decimal Tipo
    {
        get => _tipo;
        set
        {
            bool modified = SetPropertyValue(nameof(Tipo), ref _tipo, value); 
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            CalcularImporteImpuesto();
        }
    }

    [ImmediatePostData]
    public bool EsRetencion
    {
        get => _esRetencion;
        set
        {
            bool modified = SetPropertyValue(nameof(EsRetencion), ref _esRetencion, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            CalcularImporteImpuesto();
        }
    }

    private void CalcularImporteImpuesto()
    {
        ImporteImpuestos = AmountCalculator.GetTaxAmount(BaseImponible, Tipo, EsRetencion);
    }

    public Impuesto? Impuesto
    {
        get => _impuesto;
        set => SetPropertyValue(nameof(Impuesto), ref _impuesto, value);
    }
    public ClaveRegimen? RegimenFiscal
    {
        get => _regimenFiscal;
        set => SetPropertyValue(nameof(RegimenFiscal), ref _regimenFiscal, value);
    }
    
    public CalificacionOperacion? TipoOperacion
    {
        get => _tipoOperacion;
        set => SetPropertyValue(nameof(TipoOperacion), ref _tipoOperacion, value);
    }
    
    public CausaExencion? CausaExencion
    {
        get => _causaExencion;
        set => SetPropertyValue(nameof(CausaExencion), ref _causaExencion, value);
    }

    [ImmediatePostData]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set
        {
            bool modified = SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            CalcularImporteImpuesto();
        }
    }
    
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);  
    }
}