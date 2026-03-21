using System.ComponentModel;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Crm;
using erp.Module.Factories;
using VeriFactu.Xml.Factu;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Contact")]
[DefaultProperty(nameof(Nombre))]
[Appearance("BlockEditingWhenInactive", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Activo = false", Context = "Any", Enabled = false)]
public class Contacto(Session session) : EntidadBase(session)
{
    private bool _esVendedor;
    private bool _activo;
    private DateTime _fechaAlta;
    private DateTime? _fechaBaja;
    private EquipoVenta? _equipoVenta;
    private decimal _porcentajeComision;
    private decimal _importeComisionFijo;
    private ApplicationUser? _usuario;
    private Cliente? _cliente;
    private string? _codigo;
    private string? _codigoPostal;
    private string? _correoElectronico;
    private string? _direccion;
    private DateTime _expedicion;
    private DateTime? _fechaNacimiento;
    private MediaDataObject? _foto;
    private string? _movil;
    private Nacionalidad? _nacionalidad;
    private string? _nif;
    private string? _nombre;
    private string? _nombreComercial;
    private string? _notas;
    private int _numero;
    private Pais? _pais;
    private Poblacion? _poblacion;
    private Provincia? _provincia;
    private string? _sitioWeb;
    private string? _telefono;
    private IDType _tipoIdentificacion;

    [XafDisplayName("Cliente")]
    [Association("Cliente-Contactos")]
    [DataSourceCriteria("Activo = true")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [XafDisplayName("¿Es Vendedor?")]
    public bool EsVendedor
    {
        get => _esVendedor;
        set => SetPropertyValue(nameof(EsVendedor), ref _esVendedor, value);
    }

    [XafDisplayName("Activo")]
    public bool Activo
    {
        get => _activo;
        set
        {
            if (!SetPropertyValue(nameof(Activo), ref _activo, value)) return;
            if (IsLoading || IsSaving) return;
            if (value)
                FechaBaja = null;
            else
                FechaBaja = DateTime.Now;
        }
    }

    [XafDisplayName("Fecha de Alta")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime FechaAlta
    {
        get => _fechaAlta;
        set => SetPropertyValue(nameof(FechaAlta), ref _fechaAlta, value);
    }

    [XafDisplayName("Fecha de Baja")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime? FechaBaja
    {
        get => _fechaBaja;
        set => SetPropertyValue(nameof(FechaBaja), ref _fechaBaja, value);
    }

    [XafDisplayName("Equipo de Venta")]
    [Association("EquipoVenta-Vendedores")]
    [DataSourceCriteria("EsVendedor = true")]
    [ImmediatePostData]
    public EquipoVenta? EquipoVenta
    {
        get => _equipoVenta;
        set
        {
            var modified = SetPropertyValue(nameof(EquipoVenta), ref _equipoVenta, value);
            if (modified && !IsLoading && !IsSaving && value != null)
            {
                PorcentajeComision = value.PorcentajeComision;
                ImporteComisionFijo = value.ImporteComisionFijo;
            }
        }
    }

    [XafDisplayName("% Comisión")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal PorcentajeComision
    {
        get => _porcentajeComision;
        set => SetPropertyValue(nameof(PorcentajeComision), ref _porcentajeComision, value);
    }

    [XafDisplayName("Importe Fijo Comisión")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteComisionFijo
    {
        get => _importeComisionFijo;
        set => SetPropertyValue(nameof(ImporteComisionFijo), ref _importeComisionFijo, value);
    }

    [XafDisplayName("Usuario de Aplicación")]
    public ApplicationUser? Usuario
    {
        get => _usuario;
        set
        {
            if (!SetPropertyValue(nameof(Usuario), ref _usuario, value)) return;
            if (IsLoading || IsSaving || value == null) return;
            
            var otroContacto = Session.FindObject<Contacto>(PersistentCriteriaEvaluationBehavior.InTransaction,
                CriteriaOperator.Parse("Usuario = ? and Oid != ?", value, Oid));
            if (otroContacto != null)
            {
                otroContacto.Usuario = null;
            }
        }
    }

    [Size(50)]
    [XafDisplayName("Código")]
    [ModelDefault("AllowEdit", "False")]
    public string? Codigo
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
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(255)]
    [XafDisplayName("Nombre Comercial")]
    public string? NombreComercial
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
    public string? Nif
    {
        get => _nif;
        set => SetPropertyValue(nameof(Nif), ref _nif, value);
    }

    [Size(255)]
    [XafDisplayName("Dirección")]
    public string? Direccion
    {
        get => _direccion;
        set => SetPropertyValue(nameof(Direccion), ref _direccion, value);
    }

    [XafDisplayName("País")]
    public Pais? Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [DataSourceProperty("Pais.Provincias")]
    [XafDisplayName("Provincia")]
    public Provincia? Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }

    [DataSourceProperty("Provincia.Poblaciones")]
    [XafDisplayName("Población")]
    public Poblacion? Poblacion
    {
        get => _poblacion;
        set => SetPropertyValue(nameof(Poblacion), ref _poblacion, value);
    }

    [Size(10)]
    [XafDisplayName("Código Postal")]
    public string? CodigoPostal
    {
        get => _codigoPostal;
        set => SetPropertyValue(nameof(CodigoPostal), ref _codigoPostal, value);
    }

    [XafDisplayName("Teléfono")]
    public string? Telefono
    {
        get => _telefono;
        set => SetPropertyValue(nameof(Telefono), ref _telefono, value);
    }

    [XafDisplayName("Móvil")]
    public string? Movil
    {
        get => _movil;
        set => SetPropertyValue(nameof(Movil), ref _movil, value);
    }

    [Association("Nacionalidad-Contactos")]
    [XafDisplayName("Nacionalidad")]
    public Nacionalidad? Nacionalidad
    {
        get => _nacionalidad;
        set => SetPropertyValue(nameof(Nacionalidad), ref _nacionalidad, value);
    }

    [XafDisplayName("Fecha de nacimiento")]
    public DateTime? FechaNacimiento
    {
        get => _fechaNacimiento;
        set => SetPropertyValue(nameof(FechaNacimiento), ref _fechaNacimiento, value);
    }

    [XafDisplayName("Fecha expedición")]
    public DateTime Expedicion
    {
        get => _expedicion;
        set => SetPropertyValue(nameof(Expedicion), ref _expedicion, value);
    }

    [XafDisplayName("Correo Electrónico")]
    public string? CorreoElectronico
    {
        get => _correoElectronico;
        set => SetPropertyValue(nameof(CorreoElectronico), ref _correoElectronico, value);
    }

    [XafDisplayName("Sitio Web")]
    public string? SitioWeb
    {
        get => _sitioWeb;
        set => SetPropertyValue(nameof(SitioWeb), ref _sitioWeb, value);
    }

    [XafDisplayName("Foto")]
    public MediaDataObject? Foto
    {
        get => _foto;
        set => SetPropertyValue(nameof(Foto), ref _foto, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
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

    [Action(Caption = "Cambiar Estado", ConfirmationMessage = "¿Desea cambiar el estado de este contacto?", 
        SelectionDependencyType = MethodActionSelectionDependencyType.RequireSingleObject)]
    public void ToggleEstado()
    {
        Activo = !Activo;
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
    }

    public virtual bool GetAsignarCodigoAlGuardar()
    {
        return !string.IsNullOrEmpty(GetPrefijoCodigo());
    }

    public virtual void AsignarCodigo()
    {
        var prefijo = GetPrefijoCodigo();
        if (string.IsNullOrEmpty(prefijo)) return;
        var nombreSecuencia = GetType().FullName ?? GetType().Name;
        Numero = SequenceFactory.GetNextSequence(Session, nombreSecuencia, out var formattedSequence, prefijo, 5);
        Codigo = formattedSequence;
    }

    public virtual string GetPrefijoCodigo()
    {
        return string.Empty;
    }

    private void InitValues()
    {
        TipoIdentificacion = IDType.NIF_IVA;
        Activo = true;
        FechaAlta = DateTime.Now;
    }
}