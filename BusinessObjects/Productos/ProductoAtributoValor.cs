using System;
using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[XafDisplayName("Valor de Atributo de Producto")]
[DefaultProperty(nameof(Valor))]
public class ProductoAtributoValor(Session session) : EntidadBase(session)
{
    private Producto _producto = null!;
    private Atributo _atributo = null!;
    private string? _valor;
    private int _orden;

    [Association("Producto-Atributos")]
    [XafDisplayName("Producto")]
    [RuleRequiredField]
    public Producto Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    [XafDisplayName("Atributo")]
    [RuleRequiredField]
    [ImmediatePostData]
    public Atributo Atributo
    {
        get => _atributo;
        set => SetPropertyValue(nameof(Atributo), ref _atributo, value);
    }

    [XafDisplayName("Valor")]
    [Size(SizeAttribute.Unlimited)]
    [Appearance("Valor_Hide", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'TextoCorto' AND TipoDato != 'TextoLargo' AND TipoDato != 'ListaSeleccion'")]
    [Appearance("Valor_Opcion_Hide", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato == 'ListaSeleccion'")]
    [ImmediatePostData]
    public string? Valor
    {
        get => _valor;
        set
        {
            if (SetPropertyValue(nameof(Valor), ref _valor, value))
            {
                if (!IsLoading && !IsSaving)
                {
                    OnChanged(nameof(ValorLargo));
                    OnChanged(nameof(ValorEntero));
                    OnChanged(nameof(ValorDecimal));
                    OnChanged(nameof(ValorBooleano));
                    OnChanged(nameof(ValorFecha));
                    OnChanged(nameof(ValorOpcion));
                }
            }
        }
    }

    [XafDisplayName("Valor")]
    [Size(SizeAttribute.Unlimited)]
    [Appearance("ValorLargo_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'TextoLargo'")]
    [EditorAlias(EditorAliases.HtmlPropertyEditor)]
    [ImmediatePostData]
    public string? ValorLargo
    {
        get => Valor;
        set => Valor = value;
    }

    [XafDisplayName("Valor")]
    [Appearance("ValorEntero_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'Entero'")]
    [ImmediatePostData]
    public int? ValorEntero
    {
        get => int.TryParse(Valor, out int v) ? v : null;
        set => Valor = value?.ToString();
    }

    [XafDisplayName("Valor")]
    [Appearance("ValorDecimal_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'Decimal'")]
    [ImmediatePostData]
    public decimal? ValorDecimal
    {
        get => decimal.TryParse(Valor, out decimal v) ? v : null;
        set => Valor = value?.ToString();
    }

    [XafDisplayName("Valor")]
    [Appearance("ValorBooleano_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'Booleano'")]
    [ImmediatePostData]
    public bool? ValorBooleano
    {
        get => bool.TryParse(Valor, out bool v) ? v : null;
        set => Valor = value?.ToString();
    }

    [XafDisplayName("Valor")]
    [Appearance("ValorFecha_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'Fecha'")]
    [ImmediatePostData]
    public DateTime? ValorFecha
    {
        get => DateTime.TryParse(Valor, out DateTime v) ? v : null;
        set => Valor = value?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    [XafDisplayName("Valor")]
    [Appearance("ValorOpcion_Visible", Visibility = ViewItemVisibility.Hide, Criteria = "TipoDato != 'ListaSeleccion'")]
    [DataSourceProperty("Atributo.Opciones")]
    [ImmediatePostData]
    public AtributoOpcion? ValorOpcion
    {
        get => Atributo?.Opciones.FirstOrDefault(o => o.Valor == Valor);
        set => Valor = value?.Valor;
    }

    [XafDisplayName("Orden")]
    public int Orden
    {
        get => _orden;
        set => SetPropertyValue(nameof(Orden), ref _orden, value);
    }

    [XafDisplayName("Unidad")]
    public string? UnidadMedida => Atributo?.UnidadMedida;

    [XafDisplayName("Tipo de dato")]
    public TipoDatoAtributo? TipoDato => Atributo?.TipoDato;

    protected override void OnSaving()
    {
        base.OnSaving();
        ValidarValor();
    }

    private void ValidarValor()
    {
        if (Atributo == null || string.IsNullOrEmpty(Valor))
        {
            if (Atributo?.EsObligatorio == true)
                throw new UserFriendlyException($"El atributo '{Atributo.Nombre}' es obligatorio.");
            return;
        }

        switch (Atributo.TipoDato)
        {
            case TipoDatoAtributo.Entero:
                var intValNullable = ValorEntero;
                if (!intValNullable.HasValue)
                    throw new UserFriendlyException($"El valor '{Valor}' no es un número entero válido para el atributo '{Atributo.Nombre}'.");
                int intVal = intValNullable.Value;
                if (Atributo.Minimo.HasValue && intVal < Atributo.Minimo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' debe ser al menos {Atributo.Minimo.Value}.");
                if (Atributo.Maximo.HasValue && intVal > Atributo.Maximo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' no puede ser mayor que {Atributo.Maximo.Value}.");
                break;

            case TipoDatoAtributo.Decimal:
                var decValNullable = ValorDecimal;
                if (!decValNullable.HasValue)
                    throw new UserFriendlyException($"El valor '{Valor}' no es un número decimal válido para el atributo '{Atributo.Nombre}'.");
                decimal decVal = decValNullable.Value;
                if (Atributo.Minimo.HasValue && decVal < Atributo.Minimo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' debe ser al menos {Atributo.Minimo.Value}.");
                if (Atributo.Maximo.HasValue && decVal > Atributo.Maximo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' no puede ser mayor que {Atributo.Maximo.Value}.");
                break;

            case TipoDatoAtributo.Booleano:
                if (!ValorBooleano.HasValue)
                    throw new UserFriendlyException($"El valor '{Valor}' no es un valor booleano válido (True/False) para el atributo '{Atributo.Nombre}'.");
                break;

            case TipoDatoAtributo.Fecha:
                if (!ValorFecha.HasValue)
                    throw new UserFriendlyException($"El valor '{Valor}' no es una fecha válida para el atributo '{Atributo.Nombre}'.");
                break;

            case TipoDatoAtributo.TextoCorto:
            case TipoDatoAtributo.TextoLargo:
                if (Atributo.LongitudMaxima.HasValue && Valor?.Length > Atributo.LongitudMaxima.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' excede la longitud máxima de {Atributo.LongitudMaxima.Value} caracteres.");
                break;

            case TipoDatoAtributo.ListaSeleccion:
                if (ValorOpcion == null)
                    throw new UserFriendlyException($"El valor '{Valor}' no es una opción válida para el atributo '{Atributo.Nombre}'.");
                break;
        }
    }
}
