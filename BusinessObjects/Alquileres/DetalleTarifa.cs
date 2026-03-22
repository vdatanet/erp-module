using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[ImageName("BO_List")]
[XafDisplayName("Detalle Tarifa")]
public class DetalleTarifa(Session session) : EntidadBase(session)
{
    private DateTime _fechaFin;
    private DateTime _fechaInicio;
    private string? _notas;
    private decimal _precio;
    private Tarifa? _tarifa;
    private int _temporada;

    [Association("Tarifa-Detalles")]
    [XafDisplayName("Tarifa")]
    public Tarifa? Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Temporada")]
    public int Temporada
    {
        get => _temporada;
        set => SetPropertyValue(nameof(Temporada), ref _temporada, value);
    }

    [XafDisplayName("Desde")]
    [RuleRequiredField("RuleRequiredField_DetalleTarifa_Desde", DefaultContexts.Save, CustomMessageTemplate = "La Fecha de Inicio (Desde) del Detalle de Tarifa es obligatoria")]
    public DateTime Desde
    {
        get => _fechaInicio;
        set
        {
            var modified = SetPropertyValue(nameof(Desde), ref _fechaInicio, value);
            if (modified && !IsLoading)
                Temporada = _fechaInicio.Year;
        }
    }

    [XafDisplayName("Hasta")]
    [RuleRequiredField("RuleRequiredField_DetalleTarifa_Hasta", DefaultContexts.Save, CustomMessageTemplate = "La Fecha de Fin (Hasta) del Detalle de Tarifa es obligatoria")]
    public DateTime Hasta
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(Hasta), ref _fechaFin, value);
    }

    [XafDisplayName("Precio")]
    public decimal Precio
    {
        get => _precio;
        set => SetPropertyValue(nameof(Precio), ref _precio, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
}