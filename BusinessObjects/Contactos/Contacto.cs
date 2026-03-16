using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;
using erp.Module.Factories;
using Tarea = erp.Module.BusinessObjects.Planificacion.Tarea;
using VeriFactu.Xml.Factu;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Contact")]
[DefaultProperty(nameof(Nombre))]
public class Contacto(Session session) : EntidadBase(session)
{
    private string _codigo;
    private int _numero;
    private string _codigoPostal;
    private string _correoElectronico;
    private string _direccion;
    private MediaDataObject _foto;
    private string _movil;
    private IDType _tipoIdentificacion;
    private string _nif;
    private string _nombre;
    private string _nombreComercial;
    private string _notas;
    private Pais _pais;
    private Poblacion _poblacion;
    private Provincia _provincia;
    private string _sitioWeb;
    private string _telefono;
    private Cliente _cliente;

    [XafDisplayName("Cliente")]
    [Association("Cliente-Contactos")]
    public Cliente Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [Size(50)]
    [XafDisplayName("Código")]
    [ModelDefault("AllowEdit", "False")]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [XafDisplayName("Número")]
    [ModelDefault("AllowEdit", "False")]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(255)]
    [XafDisplayName("Nombre Comercial")]
    public string NombreComercial
    {
        get => _nombreComercial;
        set => SetPropertyValue(nameof(NombreComercial), ref _nombreComercial, value);
    }
    
    [NonCloneable]
    [XafDisplayName("Tipo de Identificación")]
    public IDType TipoIdentificacion
    {
        get => _tipoIdentificacion;
        set => SetPropertyValue(nameof(TipoIdentificacion), ref _tipoIdentificacion, value);
    }

    [Size(50)]
    [XafDisplayName("NIF")]
    public string Nif
    {
        get => _nif;
        set => SetPropertyValue(nameof(Nif), ref _nif, value);
    }

    [Size(255)]
    [XafDisplayName("Dirección")]
    public string Direccion
    {
        get => _direccion;
        set => SetPropertyValue(nameof(Direccion), ref _direccion, value);
    }

    [XafDisplayName("País")]
    public Pais Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [DataSourceProperty("Pais.Provincias")]
    [XafDisplayName("Provincia")]
    public Provincia Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }

    [DataSourceProperty("Provincia.Poblaciones")]
    [XafDisplayName("Población")]
    public Poblacion Poblacion
    {
        get => _poblacion;
        set => SetPropertyValue(nameof(Poblacion), ref _poblacion, value);
    }

    [Size(10)]
    [XafDisplayName("Código Postal")]
    public string CodigoPostal
    {
        get => _codigoPostal;
        set => SetPropertyValue(nameof(CodigoPostal), ref _codigoPostal, value);
    }

    [XafDisplayName("Teléfono")]
    public string Telefono
    {
        get => _telefono;
        set => SetPropertyValue(nameof(Telefono), ref _telefono, value);
    }

    [XafDisplayName("Móvil")]
    public string Movil
    {
        get => _movil;
        set => SetPropertyValue(nameof(Movil), ref _movil, value);
    }

    [XafDisplayName("Correo Electrónico")]
    public string CorreoElectronico
    {
        get => _correoElectronico;
        set => SetPropertyValue(nameof(CorreoElectronico), ref _correoElectronico, value);
    }

    [XafDisplayName("Sitio Web")]
    public string SitioWeb
    {
        get => _sitioWeb;
        set => SetPropertyValue(nameof(SitioWeb), ref _sitioWeb, value);
    }

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [XafDisplayName("Foto")]
    public MediaDataObject Foto
    {
        get => _foto;
        set => SetPropertyValue(nameof(Foto), ref _foto, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("Contacto-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("Contacto-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("Contacto-Adjuntos")]
    [XafDisplayName("Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>();
    
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if (string.IsNullOrEmpty(Codigo) && GetAsignarCodigoAlGuardar()) AsignarCodigo();
    }

    public virtual bool GetAsignarCodigoAlGuardar()
    {
        return !string.IsNullOrEmpty(GetPrefijoCodigo());
    }

    public virtual void AsignarCodigo()
    {
        var prefijo = GetPrefijoCodigo();
        if (string.IsNullOrEmpty(prefijo)) return;
        Numero = SequenceFactory.GetNextSequence(Session, GetType().FullName, out var formattedSequence, prefijo, 5);
        Codigo = formattedSequence;
    }

    public virtual string GetPrefijoCodigo()
    {
        return string.Empty;
    }

    private void InitValues()
    {
        TipoIdentificacion = IDType.NIF_IVA;
    }
}