using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Today")]
[XafDisplayName("Precio Diario")]
public class PrecioDiario(Session session) : EntidadBase(session)
{
    private Tarifa _tarifa;
    private DateTime _fecha;
    private decimal _precio;
    private string _observaciones;
    private int _p1;
    private int _p2;
    private int _p3;
    private int _temporada;

    [Association("Tarifa-PreciosDiarios")]
    [XafDisplayName("Tarifa")]
    public Tarifa Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [XafDisplayName("Fecha")]
    [RuleRequiredField]
    public DateTime Fecha
    {
        get => _fecha;
        set
        {
            var modified = SetPropertyValue(nameof(Fecha), ref _fecha, value);
            if (modified && !IsLoading && !IsSaving)
            {
                Temporada = Fecha.Year;
                CalcularPrecio();
            }
        }
    }

    [XafDisplayName("Precio")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Precio
    {
        get => _precio;
        set => SetPropertyValue(nameof(Precio), ref _precio, value);
    }

    [Size(255)]
    [XafDisplayName("Observaciones")]
    public string Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [ModelDefault("AllowEdit", "False")]
    public int P1
    {
        get => _p1;
        set => SetPropertyValue(nameof(P1), ref _p1, value);
    }

    [ModelDefault("AllowEdit", "False")]
    public int P2
    {
        get => _p2;
        set => SetPropertyValue(nameof(P2), ref _p2, value);
    }

    [ModelDefault("AllowEdit", "False")]
    public int P3
    {
        get => _p3;
        set => SetPropertyValue(nameof(P3), ref _p3, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Temporada")]
    public int Temporada
    {
        get => _temporada;
        set => SetPropertyValue(nameof(Temporada), ref _temporada, value);
    }

    private void CalcularPrecio()
    {
        if (Tarifa == null) return;
        var detalleTarifa = Session.FindObject<DetalleTarifa>(
            CriteriaOperator.Parse("Tarifa.Oid = ? AND Desde <= ? AND Hasta >= ?", Tarifa.Oid, Fecha, Fecha));
        if (detalleTarifa != null)
            Precio = detalleTarifa.Precio;
    }
}
