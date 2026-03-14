using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Impuestos;

namespace erp.Module.BusinessObjects.Base.Ventas;

[ImageName("Top10Percent")]
[DefaultProperty(nameof(TipoImpuesto))]
public class ImpuestoLineaDocumentoVenta(Session session) : EntidadBase(session)
{
    private LineaDocumentoVenta _lineaDocumentoVenta;
    private TipoImpuesto _tipoImpuesto;
    private string _nombre;
    private string _notas;
    private int _secuencia;
    private Cuenta _cuenta;
    private decimal _tipo;
    private bool _esCompuesto;
    private bool _esRetencion;
    private decimal _baseImponible;
    private decimal _importeImpuestos;

    [Association("LineaDocumentoVenta-Impuestos")]
    [XafDisplayName("Línea Documento Venta")]
    public LineaDocumentoVenta LineaDocumentoVenta
    {
        get => _lineaDocumentoVenta;
        set => SetPropertyValue(nameof(LineaDocumentoVenta), ref _lineaDocumentoVenta, value);
    }

    [XafDisplayName("Tipo Impuesto")]
    public TipoImpuesto TipoImpuesto
    {
        get => _tipoImpuesto;
        set
        {
            if (SetPropertyValue(nameof(TipoImpuesto), ref _tipoImpuesto, value))
                AplicarInstantaneaTipoImpuesto(value);
        }
    }

    private void AplicarInstantaneaTipoImpuesto(TipoImpuesto t)
    {
        if (IsLoading || IsSaving) return;

        if (t == null)
        {
            Nombre = null;
            Notas = null;
            Secuencia = 0;
            Cuenta = null;
            Tipo = 0m;
            EsCompuesto = false;
            EsRetencion = false;
            return;
        }

        Nombre = t.Nombre;
        Notas = t.Notas;
        Secuencia = t.Secuencia;
        Cuenta = t.Cuenta;
        Tipo = t.Tipo;
        EsRetencion = t.EsRetencion;
    }

    [Size(255)]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(1000)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Secuencia")]
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [XafDisplayName("Cuenta")]
    public Cuenta Cuenta
    {
        get => _cuenta;
        set => SetPropertyValue(nameof(Cuenta), ref _cuenta, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Tipo %")]
    public decimal Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    public bool EsCompuesto
    {
        get => _esCompuesto;
        set => SetPropertyValue(nameof(EsCompuesto), ref _esCompuesto, value);
    }

    [XafDisplayName("Es Retención")]
    public bool EsRetencion
    {
        get => _esRetencion;
        set => SetPropertyValue(nameof(EsRetencion), ref _esRetencion, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Impuesto")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }
}