using System;
using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
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
    public Atributo Atributo
    {
        get => _atributo;
        set => SetPropertyValue(nameof(Atributo), ref _atributo, value);
    }

    [XafDisplayName("Valor")]
    [Size(SizeAttribute.Unlimited)]
    public string? Valor
    {
        get => _valor;
        set => SetPropertyValue(nameof(Valor), ref _valor, value);
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
                if (!int.TryParse(Valor, out int intVal))
                    throw new UserFriendlyException($"El valor '{Valor}' no es un número entero válido para el atributo '{Atributo.Nombre}'.");
                if (Atributo.Minimo.HasValue && intVal < Atributo.Minimo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' debe ser al menos {Atributo.Minimo.Value}.");
                if (Atributo.Maximo.HasValue && intVal > Atributo.Maximo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' no puede ser mayor que {Atributo.Maximo.Value}.");
                break;

            case TipoDatoAtributo.Decimal:
                if (!decimal.TryParse(Valor, out decimal decVal))
                    throw new UserFriendlyException($"El valor '{Valor}' no es un número decimal válido para el atributo '{Atributo.Nombre}'.");
                if (Atributo.Minimo.HasValue && decVal < Atributo.Minimo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' debe ser al menos {Atributo.Minimo.Value}.");
                if (Atributo.Maximo.HasValue && decVal > Atributo.Maximo.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' no puede ser mayor que {Atributo.Maximo.Value}.");
                break;

            case TipoDatoAtributo.Booleano:
                if (!bool.TryParse(Valor, out _))
                    throw new UserFriendlyException($"El valor '{Valor}' no es un valor booleano válido (True/False) para el atributo '{Atributo.Nombre}'.");
                break;

            case TipoDatoAtributo.Fecha:
                if (!DateTime.TryParse(Valor, out _))
                    throw new UserFriendlyException($"El valor '{Valor}' no es una fecha válida para el atributo '{Atributo.Nombre}'.");
                break;

            case TipoDatoAtributo.TextoCorto:
            case TipoDatoAtributo.TextoLargo:
                if (Atributo.LongitudMaxima.HasValue && Valor.Length > Atributo.LongitudMaxima.Value)
                    throw new UserFriendlyException($"El valor para '{Atributo.Nombre}' excede la longitud máxima de {Atributo.LongitudMaxima.Value} caracteres.");
                break;

            case TipoDatoAtributo.ListaSeleccion:
                if (!Atributo.Opciones.Any(o => o.Valor == Valor))
                    throw new UserFriendlyException($"El valor '{Valor}' no es una opción válida para el atributo '{Atributo.Nombre}'.");
                break;
        }
    }
}
