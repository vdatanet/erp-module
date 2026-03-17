using System.ComponentModel;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Contact")]
[DefaultProperty(nameof(NombreCompleto))]
public class Viajero(Session session) : Contacto(session)
{
    private Parentesco? _parentesco;
    private DateTime _expedicion;
    private Cliente.Sexes _sexo;
    private Cliente.TiposDocumentos _tipoDocumento;
    private string _mail;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Sexo = Cliente.Sexes.Femenino;
        TipoDocumento = Cliente.TiposDocumentos.Nif;
        var nacionalidad = Session.FindObject<Nacionalidad>(CriteriaOperator.Parse("Nombre = 'Española'"));
        if (nacionalidad != null) Nacionalidad = nacionalidad;
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);

        if (!IsLoading && !IsSaving && propertyName == nameof(Cliente) && newValue is Cliente cliente)
        {
            if (cliente.Nif != null) Nif = cliente.Nif;
            if (cliente.Nombre != null) Nombre = cliente.Nombre;
            if (cliente.NombreComercial != null) NombreComercial = cliente.NombreComercial;
            if (cliente.Direccion != null) Direccion = cliente.Direccion;
            if (cliente.CodigoPostal != null) CodigoPostal = cliente.CodigoPostal;
            if (cliente.Provincia != null) Provincia = cliente.Provincia;
            if (cliente.Pais != null) Pais = cliente.Pais;
            if (cliente.Telefono != null) base.Telefono = cliente.Telefono;
            if (cliente.Movil != null) base.Movil = cliente.Movil;
            if (cliente.CorreoElectronico != null) CorreoElectronico = cliente.CorreoElectronico;
            if (cliente.Notas != null) Notas = cliente.Notas;
        }
    }

    [XafDisplayName("Fecha expedición")]
    public DateTime Expedicion
    {
        get => _expedicion;
        set => SetPropertyValue(nameof(Expedicion), ref _expedicion, value);
    }

    [XafDisplayName("Sexo")]
    public Cliente.Sexes Sexo
    {
        get => _sexo;
        set => SetPropertyValue(nameof(Sexo), ref _sexo, value);
    }

    [XafDisplayName("Tipo documento identificativo")]
    public Cliente.TiposDocumentos TipoDocumento
    {
        get => _tipoDocumento;
        set => SetPropertyValue(nameof(TipoDocumento), ref _tipoDocumento, value);
    }

    [XafDisplayName("Parentesco")]
    public Parentesco? Parentesco
    {
        get => _parentesco;
        set => SetPropertyValue(nameof(Parentesco), ref _parentesco, value);
    }

    [XafDisplayName("Correo electrónico")]
    public string Email
    {
        get => _mail;
        set => SetPropertyValue(nameof(Email), ref _mail, value);
    }

    [Association("Reserva-Viajeros")]
    [XafDisplayName("Reservas")]
    public XPCollection<Reserva> Reservas => GetCollection<Reserva>();
}
