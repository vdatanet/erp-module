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
[XafDisplayName("Preu Diari")]
public class PreuDiari(Session session) : EntidadBase(session)
{
    private Tarifa _tarifa;
    private DateTime _data;
    private decimal _preu;
    private string _observacions;
    private int _p1;
    private int _p2;
    private int _p3;
    private int _temporada;

    [Association("Tarifa-PreusDiaris")]
    [XafDisplayName("Tarifa")]
    public Tarifa Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [XafDisplayName("Data")]
    [RuleRequiredField]
    public DateTime Data
    {
        get => _data;
        set
        {
            var modified = SetPropertyValue(nameof(Data), ref _data, value);
            if (modified && !IsLoading && !IsSaving)
            {
                Temporada = Data.Year;
                CalcularPreu();
            }
        }
    }

    [XafDisplayName("Preu")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Preu
    {
        get => _preu;
        set => SetPropertyValue(nameof(Preu), ref _preu, value);
    }

    [Size(255)]
    [XafDisplayName("Observacions")]
    public string Observacions
    {
        get => _observacions;
        set => SetPropertyValue(nameof(Observacions), ref _observacions, value);
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

    private void CalcularPreu()
    {
        if (Tarifa == null) return;
        var detallTarifa = Session.FindObject<DetallTarifa>(
            CriteriaOperator.Parse("Tarifa.Oid = ? AND Desde <= ? AND Fins >= ?", Tarifa.Oid, Data, Data));
        if (detallTarifa != null)
            Preu = detallTarifa.Preu;
    }
}
