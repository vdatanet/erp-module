using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using Country = erp.Module.BusinessObjects.Common.Country;
using State = erp.Module.BusinessObjects.Common.State;
using Task = erp.Module.BusinessObjects.Planning.Task;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Contact")]
[DefaultProperty(nameof(Nombre))]
public class Contact(Session session) : BaseEntity(session)
{
    private string _nombre;
    private string _nombreComercial;
    private string _nif;
    private string _direccion;
    private Country _pais;
    private State _provincia;
    private City _poblacion;
    private string _telefono;
    private string _movil;
    private string _correoElectronico;
    private string _sitioWeb;
    private MediaDataObject _foto;
    private string _notas;

    [Size(255)]
    [RuleRequiredField]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(255)]
    public string NombreComercial
    {
        get => _nombreComercial;
        set => SetPropertyValue(nameof(NombreComercial), ref _nombreComercial, value);
    }

    [Size(50)]
    [RuleRequiredField]
    public string Nif
    {
        get => _nif;
        set => SetPropertyValue(nameof(Nif), ref _nif, value);
    }

    [Size(255)]
    public string Direccion
    {
        get => _direccion;
        set => SetPropertyValue(nameof(Direccion), ref _direccion, value);
    }

    public Country Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [DataSourceProperty("Pais.States")]
    public State Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }

    [DataSourceProperty("Provincia.Cities")]
    public City Poblacion
    {
        get => _poblacion;
        set => SetPropertyValue(nameof(Poblacion), ref _poblacion, value);
    }

    public string Telefono
    {
        get => _telefono;
        set => SetPropertyValue(nameof(Telefono), ref _telefono, value);
    }

    public string Movil
    {
        get => _movil;
        set => SetPropertyValue(nameof(Movil), ref _movil, value);
    }

    public string CorreoElectronico
    {
        get => _correoElectronico;
        set => SetPropertyValue(nameof(CorreoElectronico), ref _correoElectronico, value);
    }

    public string SitioWeb
    {
        get => _sitioWeb;
        set => SetPropertyValue(nameof(SitioWeb), ref _sitioWeb, value);
    }
    
    public MediaDataObject Foto
    {
        get => _foto;
        set => SetPropertyValue(nameof(Foto), ref _foto, value);
    }
    
    [Size(SizeAttribute.Unlimited)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
    
    [Aggregated]
    [Association("Contact-Tasks")]
    public XPCollection<Task> Tasks => GetCollection<Task>(nameof(Tasks));
    
    [Aggregated]
    [Association("Contact-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>(nameof(Pictures));
    
    [Aggregated]
    [Association("Contact-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>(nameof(Attachments));
}