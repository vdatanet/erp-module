using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Address")]
public class Domicilio(Session session) : EntidadBase(session)
{
    private Cliente? _clienteEnvio;
    private string? _direccion;
    private Pais? _pais;
    private Provincia? _provincia;
    private Poblacion? _poblacion;
    private string? _codigoPostal;
    private string? _telefono;
    private string? _movil;
    private string? _correoElectronico;

    [XafDisplayName("Cliente (Dirección Envío)")]
    [Association("Cliente-DireccionesEnvio")]
    public Cliente? ClienteEnvio
    {
        get => _clienteEnvio;
        set => SetPropertyValue(nameof(ClienteEnvio), ref _clienteEnvio, value);
    }

    [Size(255)]
    [XafDisplayName("Dirección")]
    public string? Direccion
    {
        get => _direccion;
        set => SetPropertyValue(nameof(Direccion), ref _direccion, value);
    }

    [XafDisplayName("País")]
    [ImmediatePostData]
    public Pais? Pais
    {
        get => _pais;
        set
        {
            if (!SetPropertyValue(nameof(Pais), ref _pais, value)) return;
            if (IsLoading || IsSaving) return;
            if (value == null)
            {
                Provincia = null;
                Poblacion = null;
            }
        }
    }

    [DataSourceProperty("Pais.Provincias", DataSourcePropertyIsNullMode.SelectAll)]
    [XafDisplayName("Provincia")]
    [ImmediatePostData]
    public Provincia? Provincia
    {
        get => _provincia;
        set
        {
            if (!SetPropertyValue(nameof(Provincia), ref _provincia, value)) return;
            if (IsLoading || IsSaving) return;
            if (value != null)
            {
                Pais = value.Pais;
            }
            else
            {
                Poblacion = null;
            }
        }
    }

    [DataSourceProperty("Provincia.Poblaciones", DataSourcePropertyIsNullMode.SelectAll)]
    [XafDisplayName("Población")]
    [ImmediatePostData]
    public Poblacion? Poblacion
    {
        get => _poblacion;
        set
        {
            if (!SetPropertyValue(nameof(Poblacion), ref _poblacion, value)) return;
            if (IsLoading || IsSaving) return;
            if (value != null)
            {
                Provincia = value.Provincia;
            }
        }
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

    [XafDisplayName("Correo Electrónico")]
    public string? CorreoElectronico
    {
        get => _correoElectronico;
        set => SetPropertyValue(nameof(CorreoElectronico), ref _correoElectronico, value);
    }
}