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
[ImageName("BO_Organization")]
public class Tercero(Session session) : Contacto(session)
{
    private CuentaContable? _cuentaContable;

    [XafDisplayName("CuentaContable Contable")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public CuentaContable? CuentaContable
    {
        get => _cuentaContable;
        set => SetPropertyValue(nameof(CuentaContable), ref _cuentaContable, value);
    }

    [Association("Tercero-DocumentosVenta")]
    [XafDisplayName("Documentos de Venta")]
    [VisibleInDetailView(false)]
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();

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
        AsignarCuentaContable();
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        AsignarCuentaContable();
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

        // Solo se debe intentar crear/asignar cuenta contable en Cliente, Proveedor o Acreedor
        if (!(this is Cliente) && !(this is Proveedor) && !(this is Acreedor)) return;

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
        if (config == null) return;

        CuentaContable? cuentaDefecto = null;
        CuentaContable? cuentaPadre = null;

        if (this is Cliente)
        {
            cuentaDefecto = config.CuentaClientesPorDefecto;
            cuentaPadre = config.CuentaPadreClientes;
        }
        else if (this is Proveedor)
        {
            cuentaDefecto = config.CuentaProveedoresPorDefecto;
            cuentaPadre = config.CuentaPadreProveedores;
        }
        else if (this is Acreedor)
        {
            cuentaDefecto = config.CuentaAcreedoresPorDefecto;
            cuentaPadre = config.CuentaPadreAcreedores;
        }

        if (cuentaDefecto != null && cuentaDefecto.EstaActiva && cuentaDefecto.EsAsentable)
        {
            CuentaContable = cuentaDefecto;
        }
        else if (cuentaPadre != null)
        {
            if (string.IsNullOrEmpty(Codigo))
            {
                AsignarCodigo();
            }

            if (string.IsNullOrEmpty(Codigo))
            {
                // En AfterConstruction el código suele estar vacío.
                // Si estamos en OnSaving y sigue vacío, algo va mal.
                if (IsSaving) return;
                return;
            }

            // Extraer solo la parte numérica del código del tercero (ej. C/0001 -> 0001)
            var suffix = new string((Codigo ?? "").Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(suffix))
            {
                // Si no hay dígitos, usamos el número de registro como respaldo
                suffix = Numero.ToString();
            }

            var prefix = cuentaPadre.Codigo ?? "";
            string cuentaCodigo;

            int paddingValue = config.PaddingNumero;
            int paddingLength = paddingValue - prefix.Length;
            if (paddingLength > 0)
            {
                cuentaCodigo = prefix + suffix.PadLeft(paddingLength, '0');
            }
            else
            {
                cuentaCodigo = prefix + suffix;
            }

            // Asegurar longitud de paddingValue si el prefijo + sufijo exceden o no llegan por alguna razón
            if (cuentaCodigo.Length > paddingValue)
            {
                // Si excede, tomamos los últimos dígitos necesarios para completar con el prefijo
                // O simplemente truncamos/ajustamos.
                cuentaCodigo = cuentaCodigo.Substring(0, paddingValue);
            }

            var cuentaExistente = Session.FindObject<CuentaContable>(new BinaryOperator(nameof(CuentaContable.Codigo), cuentaCodigo));
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
                    Codigo = cuentaCodigo,
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
        else
        {
            // Si no hay cuenta por defecto ni cuenta padre, dejamos la cuenta contable en blanco
            // Se elimina la excepción UserFriendlyException según el nuevo requerimiento
        }
    }
}