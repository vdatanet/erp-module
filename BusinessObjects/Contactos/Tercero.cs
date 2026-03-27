using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[XafDisplayName("Tercero")]
[ImageName("BO_Organization")]
public class Tercero(Session session) : Contacto(session)
{
    private CuentaContable? _cuentaContable;

    [XafDisplayName("Cuenta Contable Contable")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public CuentaContable? CuentaContable
    {
        get => _cuentaContable;
        set => SetPropertyValue(nameof(CuentaContable), ref _cuentaContable, value);
    }



    [Association("Tercero-DocumentosCompra")]
    [XafDisplayName("Documentos de Compra")]
    [VisibleInDetailView(false)]
    public XPCollection<DocumentoCompra> DocumentosCompra => GetCollection<DocumentoCompra>();

    [XafDisplayName("¿Puede Participar en Ventas?")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool IsIPuedeParticiparEnVentas => this is IPuedeParticiparEnVentas;

    [XafDisplayName("¿Puede Participar en Compras?")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool IsIPuedeParticiparEnCompras => this is IPuedeParticiparEnCompras;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    protected virtual void InitValues()
    {
        // Ya no asignamos cuenta contable aquí para evitar el consumo prematuro de secuencias de código.
        // La asignación se realizará en OnSaving.
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(Codigo))
        {
            AsignarCodigo();
        }
        AsignarCuentaContable();
        base.OnSaving();
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (!IsLoading && propertyName == nameof(Nombre) && CuentaContable != null)
        {
            if (CuentaContable.Nombre != Nombre)
            {
                CuentaContable.Nombre = Nombre;
            }
        }
    }

    private void AsignarCuentaContable()
    {
        if (IsLoading) return;

        if (!(this is Cliente || this is Proveedor || this is Acreedor))
        {
            return;
        }

        if (CuentaContable != null)
        {
            // Sincronizar el nombre si la cuenta ya existe
            if (CuentaContable.Nombre != Nombre)
            {
                CuentaContable.Nombre = Nombre;
            }

            return;
        }

        var config = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (config == null)
        {
            return;
        }

        CuentaContable? cuentaPadre = null;

        if (this is Cliente)
        {
            cuentaPadre = config.CuentaPadreClientes;
        }
        else if (this is Proveedor)
        {
            cuentaPadre = config.CuentaPadreProveedores;
        }
        else if (this is Acreedor)
        {
            cuentaPadre = config.CuentaPadreAcreedores;
        }

        if (cuentaPadre == null)
        {
            return;
        }

        // Aseguramos que tenemos código y número antes de proceder
        if (string.IsNullOrEmpty(Codigo))
        {
            AsignarCodigo();
        }
        
        // Si tras forzarlo sigue vacío, o si el número es 0, algo va muy mal
        if (string.IsNullOrEmpty(Codigo) || Numero == 0)
        {
            return;
        }

        // Extraer solo la parte numérica del código del tercero (ej. C/2026/0001 -> 0001)
        // Usamos Numero directamente si está disponible para evitar problemas con el formato del código
        var suffix = Numero > 0 ? Numero.ToString() : string.Empty;
        
        if (string.IsNullOrEmpty(suffix))
        {
            var codigoLimpio = (Codigo ?? "").Split('/').Last();
            suffix = new string(codigoLimpio.Where(char.IsDigit).ToArray());
            // Limpiar ceros iniciales
            suffix = suffix.TrimStart('0');
            if (string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(Codigo)) suffix = "0";
        }

        // Si aún no tenemos sufijo, no podemos generar la cuenta
        if (string.IsNullOrEmpty(suffix))
        {
            return;
        }

        var prefix = cuentaPadre.Codigo ?? "";
        
        // Si usamos el setter de CuentaContable.Codigo con un punto, él mismo aplicará el padding
        // Ejemplo: "430.1" -> "4300000001" (si padding es 10)
        string cuentaCodigoParaBusqueda = $"{prefix}.{suffix}";
        // Como FindObject compara con el valor en BD (que ya tiene padding), necesitamos saber el código final
        int totalPadding = config.PaddingCuentaContable;
        int ceros = totalPadding - prefix.Length - suffix.Length;
        string cuentaCodigoFinal = ceros > 0 ? prefix + new string('0', ceros) + suffix : prefix + suffix;

        var cuentaExistente =
            Session.FindObject<CuentaContable>(new BinaryOperator(nameof(CuentaContable.Codigo), cuentaCodigoFinal));
        if (cuentaExistente != null)
        {
            CuentaContable = cuentaExistente;
            if (CuentaContable.Nombre != Nombre)
            {
                CuentaContable.Nombre = Nombre;
            }
        }
        else
        {
            var nuevaCuenta = new CuentaContable(Session)
            {
                Codigo = cuentaCodigoParaBusqueda,
                Nombre = Nombre,
                CuentaPadre = cuentaPadre,
                EsAsentable = true,
                EstaActiva = true,
                Tipo = cuentaPadre.Tipo,
                Naturaleza = cuentaPadre.Naturaleza
            };
            CuentaContable = nuevaCuenta;
        }
    }
}