using System.ComponentModel;
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

    [Association("LineaDocumentoVenta-Taxes")]
    public LineaDocumentoVenta LineaDocumentoVenta
    {
        get => _lineaDocumentoVenta;
        set => SetPropertyValue(nameof(LineaDocumentoVenta), ref _lineaDocumentoVenta, value);
    }

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
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(1000)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    public Cuenta Cuenta
    {
        get => _cuenta;
        set => SetPropertyValue(nameof(Cuenta), ref _cuenta, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
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

    public bool EsRetencion
    {
        get => _esRetencion;
        set => SetPropertyValue(nameof(EsRetencion), ref _esRetencion, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
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