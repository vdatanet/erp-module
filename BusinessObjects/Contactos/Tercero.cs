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
    private Cuenta? _cuentaContable;

    [XafDisplayName("Cuenta Contable")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta? CuentaContable
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

    private void AsignarCuentaContable()
    {
        if (IsLoading) return;

        if (CuentaContable != null) return;

        var config = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (config == null) return;

        Cuenta? cuentaDefecto = null;
        Cuenta? cuentaPadre = null;

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

            var cuentaExistente = Session.FindObject<Cuenta>(new BinaryOperator(nameof(Cuenta.Codigo), Codigo));
            if (cuentaExistente != null)
            {
                CuentaContable = cuentaExistente;
            }
            else
            {
                var nuevaCuenta = new Cuenta(Session)
                {
                    Codigo = Codigo,
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
            // Si no hay cuenta por defecto ni cuenta padre, se debería impedir la creación según el resumen
            // pero XPO OnSaving no es el mejor sitio para lanzar excepciones de validación que detengan la UI de forma amigable.
            // No obstante, el requerimiento dice "impedir la creación y mostrar error de configuración".
            throw new UserFriendlyException("No se ha configurado una Cuenta Contable por defecto ni una Cuenta Padre para este tipo de tercero en la Información de la Empresa.");
        }
    }
}