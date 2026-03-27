using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

public enum TipoDatoAtributo
{
    [XafDisplayName("Texto corto")] TextoCorto,
    [XafDisplayName("Texto largo")] TextoLargo,
    [XafDisplayName("Entero")] Entero,
    [XafDisplayName("Decimal")] Decimal,
    [XafDisplayName("Booleano")] Booleano,
    [XafDisplayName("Fecha")] Fecha,
    [XafDisplayName("Lista de selección")] ListaSeleccion
}

[DefaultClassOptions]
[NavigationItem("Productos")]
[XafDisplayName("Atributo")]
[DefaultProperty(nameof(Nombre))]
public class Atributo(Session session) : EntidadBase(session)
{
    private string _nombre = string.Empty;
    private TipoDatoAtributo? _tipoDato;
    private string? _unidadMedida;
    private bool _esObligatorio;
    private int? _longitudMaxima;
    private decimal? _minimo;
    private decimal? _maximo;
    private int? _decimales;

    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [RuleRequiredField]
    [XafDisplayName("Tipo de dato")]
    [ImmediatePostData]
    public TipoDatoAtributo? TipoDato
    {
        get => _tipoDato;
        set => SetPropertyValue(nameof(TipoDato), ref _tipoDato, value);
    }

    [XafDisplayName("Unidad de medida")]
    public string? UnidadMedida
    {
        get => _unidadMedida;
        set => SetPropertyValue(nameof(UnidadMedida), ref _unidadMedida, value);
    }

    [XafDisplayName("Es obligatorio")]
    public bool EsObligatorio
    {
        get => _esObligatorio;
        set => SetPropertyValue(nameof(EsObligatorio), ref _esObligatorio, value);
    }

    [XafDisplayName("Longitud máxima")]
    [Appearance("LongitudMaxima_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'TextoCorto' AND TipoDato != 'TextoLargo'")]
    public int? LongitudMaxima
    {
        get => _longitudMaxima;
        set => SetPropertyValue(nameof(LongitudMaxima), ref _longitudMaxima, value);
    }

    [XafDisplayName("Mínimo")]
    [Appearance("Minimo_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'Entero' AND TipoDato != 'Decimal'")]
    public decimal? Minimo
    {
        get => _minimo;
        set => SetPropertyValue(nameof(Minimo), ref _minimo, value);
    }

    [XafDisplayName("Máximo")]
    [Appearance("Maximo_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'Entero' AND TipoDato != 'Decimal'")]
    public decimal? Maximo
    {
        get => _maximo;
        set => SetPropertyValue(nameof(Maximo), ref _maximo, value);
    }

    [XafDisplayName("Decimales")]
    [Appearance("Decimales_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'Decimal'")]
    public int? Decimales
    {
        get => _decimales;
        set => SetPropertyValue(nameof(Decimales), ref _decimales, value);
    }

    [Association("Atributo-Opciones")]
    [XafDisplayName("Opciones")]
    [Appearance("Opciones_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'ListaSeleccion'")]
    public XPCollection<AtributoOpcion> Opciones => GetCollection<AtributoOpcion>();
}
