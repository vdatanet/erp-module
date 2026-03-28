using System.ComponentModel;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Base.Facturacion;
using VeriFactu.Xml.Factu;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Contact")]
[DefaultProperty(nameof(Nombre))]
public class Viajero(Session session) : Contacto(session)
{
    public enum Sexos
    {
        [XafDisplayName("Masculino")] Masculino,
        [XafDisplayName("Femenino")] Femenino
    }

    private Parentesco? _parentesco;
    private Sexos _sexo;

    [XafDisplayName("Sexo")]
    public Sexos Sexo
    {
        get => _sexo;
        set => SetPropertyValue(nameof(Sexo), ref _sexo, value);
    }

    [XafDisplayName("Parentesco")]
    public Parentesco? Parentesco
    {
        get => _parentesco;
        set => SetPropertyValue(nameof(Parentesco), ref _parentesco, value);
    }

    [Association("Reserva-Viajeros")]
    [XafDisplayName("Reservas")]
    public XPCollection<Reserva> Reservas => GetCollection<Reserva>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Sexo = Sexos.Femenino;
        TipoIdentificacion = TipoIdentificacionAmigable.NIF_IVA;
        var nacionalidad = Session.FindObject<Nacionalidad>(CriteriaOperator.Parse("Nombre = 'Española'"));
        if (nacionalidad != null) Nacionalidad = nacionalidad;
    }
}