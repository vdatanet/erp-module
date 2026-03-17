using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_List")]
[XafDisplayName("Detall de Tarifa")]
public class DetallTarifa(Session session) : EntidadBase(session)
{
    private Tarifa _tarifa;
    private DateTime _dataInici;
    private DateTime _dataFi;
    private decimal _preu;
    private string _observacions;
    private int _temporada;

    [Association("Tarifa-Detalls")]
    [XafDisplayName("Tarifa")]
    public Tarifa Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [XafDisplayName("Des del")]
    [RuleRequiredField]
    public DateTime Desde
    {
        get => _dataInici;
        set
        {
            bool modified = SetPropertyValue(nameof(Desde), ref _dataInici, value);
            if (modified && !IsLoading)
                Temporada = _dataInici.Year;
        }
    }

    [XafDisplayName("Fins")]
    [RuleRequiredField]
    public DateTime Fins
    {
        get => _dataFi;
        set => SetPropertyValue(nameof(Fins), ref _dataFi, value);
    }

    [XafDisplayName("Preu")]
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
    [XafDisplayName("Temporada")]
    public int Temporada
    {
        get => _temporada;
        set => SetPropertyValue(nameof(Temporada), ref _temporada, value);
    }
}
