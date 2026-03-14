using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;
using Tarea = erp.Module.BusinessObjects.Planificacion.Tarea;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Contacto")]
[DefaultProperty(nameof(Nombre))]
public class Contacto(Session session) : EntidadBase(session)
{
    private string _nombre;
    private string _nombreComercial;
    private string _nif;
    private string _direccion;
    private Pais _pais;
    private Provincia _provincia;
    private Poblacion _poblacion;
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

    public Pais Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [DataSourceProperty("Pais.Provincias")]
    public Provincia Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }

    [DataSourceProperty("Provincia.Poblaciones")]
    public Poblacion Poblacion
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
    [Association("Contacto-Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>(nameof(Tareas));
    
    [Aggregated]
    [Association("Contacto-Pictures")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>(nameof(Imagenes));
    
    [Aggregated]
    [Association("Contacto-Attachments")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>(nameof(Adjuntos));
}