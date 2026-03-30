using erp.Module.BusinessObjects.Alquileres;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Compras;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects.Servicios.PartesTrabajo;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Factories;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Inventario;
using erp.Module.Services.Facturacion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace erp.Module.Services.Setup;

public class InformacionEmpresaSetupService(IObjectSpace objectSpace, IServiceProvider serviceProvider)
{
    private readonly ILogger<InformacionEmpresaSetupService> _logger = serviceProvider.GetService<ILogger<InformacionEmpresaSetupService>>()!;
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            return compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(InformacionEmpresa))) ?? objectSpace;
        }

        return objectSpace;
    }

    public void CreateInitialInformacionEmpresa(string? tenantName = null)
    {
        if (!OS.IsKnownType(typeof(InformacionEmpresa))) return;

        CreateInitialUnidadesFacturacion();

        var informacionEmpresa = OS.FirstOrDefault<InformacionEmpresa>(i => true);
        if (informacionEmpresa == null)
        {
            informacionEmpresa = OS.CreateObject<InformacionEmpresa>();
            informacionEmpresa.ActivarVeriFactu = false;
        }

        // Siempre establecemos estos valores o nos aseguramos de que existan
        
        if (string.IsNullOrEmpty(informacionEmpresa.Nombre)) informacionEmpresa.Nombre = "Empresa de Demostración";
        if (string.IsNullOrEmpty(informacionEmpresa.Nif)) informacionEmpresa.Nif = "12345678Z";
        informacionEmpresa.TipoIdentificacion = erp.Module.BusinessObjects.Base.Facturacion.TipoIdentificacionAmigable.NIF_IVA;
        //if (string.IsNullOrEmpty(informacionEmpresa.NombreComercial)) informacionEmpresa.NombreComercial = "vdata.net";
        //if (string.IsNullOrEmpty(informacionEmpresa.Direccion)) informacionEmpresa.Direccion = "C/. Vilamar, 2A";
        //if (string.IsNullOrEmpty(informacionEmpresa.CodigoPostal)) informacionEmpresa.CodigoPostal = "43820";
        
        //informacionEmpresa.Pais ??= OS.FirstOrDefault<Pais>(p => p.Nombre == "España");
        //informacionEmpresa.Provincia ??= OS.FirstOrDefault<Provincia>(p => p.Nombre == "Tarragona");
        //informacionEmpresa.Poblacion ??= OS.FirstOrDefault<Poblacion>(p => p.Nombre == "Calafell");

        if (informacionEmpresa.AlmacenPorDefecto == null)
        {
            var almacen = OS.FirstOrDefault<Almacen>(a => a.Codigo == "GEN") ?? OS.CreateObject<Almacen>();
            if (string.IsNullOrEmpty(almacen.Codigo)) almacen.Codigo = "GEN";
            if (string.IsNullOrEmpty(almacen.Nombre)) almacen.Nombre = "Almacén General";
            informacionEmpresa.AlmacenPorDefecto = almacen;
        }

        //if (string.IsNullOrEmpty(informacionEmpresa.Telefono)) informacionEmpresa.Telefono = "977 69 21 16";
        //if (string.IsNullOrEmpty(informacionEmpresa.CorreoElectronico)) informacionEmpresa.CorreoElectronico = "info@vdata.net";
        //if (string.IsNullOrEmpty(informacionEmpresa.SitioWeb)) informacionEmpresa.SitioWeb = "https://www.vdata.net";
        
        if (string.IsNullOrEmpty(informacionEmpresa.NombreReporteTicket)) informacionEmpresa.NombreReporteTicket = "Ticket Factura Simplificada";
        
        if (string.IsNullOrEmpty(informacionEmpresa.NombreArchivoConfigVeriFactu) && !string.IsNullOrEmpty(tenantName))
        {
            informacionEmpresa.NombreArchivoConfigVeriFactu = tenantName.Replace(".", "_") + ".cfg";
        }

        if (string.IsNullOrEmpty(informacionEmpresa.NombreSistemaVeriFactu)) informacionEmpresa.NombreSistemaVeriFactu = "VDATA ERP";
        if (string.IsNullOrEmpty(informacionEmpresa.VersionSistemaVeriFactu)) informacionEmpresa.VersionSistemaVeriFactu = "1.0.0";
        if (string.IsNullOrEmpty(informacionEmpresa.NombreAdministradorSistemaVeriFactu)) informacionEmpresa.NombreAdministradorSistemaVeriFactu = "Joan Pallàs Ribes";
        if (string.IsNullOrEmpty(informacionEmpresa.NifAdministradorSistemaVeriFactu)) informacionEmpresa.NifAdministradorSistemaVeriFactu = "43725645T";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoUrlVeriFactu)) informacionEmpresa.PrefijoUrlVeriFactu = erp.Module.BusinessObjects.Base.Facturacion.VeriFactuEndPointPrefixes.Prod;
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoUrlValidacionVeriFactu)) informacionEmpresa.PrefijoUrlValidacionVeriFactu = erp.Module.BusinessObjects.Base.Facturacion.VeriFactuEndPointPrefixes.ProdValidate;
        if (string.IsNullOrEmpty(informacionEmpresa.TextoDefectoVeriFactu)) informacionEmpresa.TextoDefectoVeriFactu = "Servicios de consultoría y asesoramiento técnico correspondientes al periodo....";
        
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAsientosPorDefecto)) informacionEmpresa.PrefijoAsientosPorDefecto = "AS";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoOfertasCompraPorDefecto)) informacionEmpresa.PrefijoOfertasCompraPorDefecto = "CO";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoPedidosCompraPorDefecto)) informacionEmpresa.PrefijoPedidosCompraPorDefecto = "CP";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAlbaranesCompraPorDefecto)) informacionEmpresa.PrefijoAlbaranesCompraPorDefecto = "CA";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasCompraPorDefecto)) informacionEmpresa.PrefijoFacturasCompraPorDefecto = "CF";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoOfertasVentaPorDefecto)) informacionEmpresa.PrefijoOfertasVentaPorDefecto = "VO";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoPedidosPorDefecto)) informacionEmpresa.PrefijoPedidosPorDefecto = "VP";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAlbaranesVentaPorDefecto)) informacionEmpresa.PrefijoAlbaranesVentaPorDefecto = "VA";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasVentaPorDefecto)) informacionEmpresa.PrefijoFacturasVentaPorDefecto = "VF";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto)) informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto = "VS";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoSesionTpvPorDefecto)) informacionEmpresa.PrefijoSesionTpvPorDefecto = "TS";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoVentaTpvPorDefecto)) informacionEmpresa.PrefijoVentaTpvPorDefecto = "TV";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoParteTrabajoPorDefecto)) informacionEmpresa.PrefijoParteTrabajoPorDefecto = "PT";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoClientes)) informacionEmpresa.PrefijoClientes = "TC";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoProveedores)) informacionEmpresa.PrefijoProveedores = "TP";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAcreedores)) informacionEmpresa.PrefijoAcreedores = "TA";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoEmpleados)) informacionEmpresa.PrefijoEmpleados = "TE";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoReservas)) informacionEmpresa.PrefijoReservas = "AR";

        if (informacionEmpresa.PaddingNumero == 0) informacionEmpresa.PaddingNumero = 5;
        if (informacionEmpresa.PaddingCuentaContable == 0) informacionEmpresa.PaddingCuentaContable = 10;
        
        int paddingCC = informacionEmpresa.PaddingCuentaContable;

        informacionEmpresa.CuentaPadreClientes ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "430".PadRight(paddingCC, '0').Substring(0, 5) || c.Codigo == "430" || c.Codigo == "43000");
        informacionEmpresa.CuentaPadreProveedores ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "400".PadRight(paddingCC, '0').Substring(0, 5) || c.Codigo == "400" || c.Codigo == "40000");
        informacionEmpresa.CuentaPadreAcreedores ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "410".PadRight(paddingCC, '0').Substring(0, 5) || c.Codigo == "410" || c.Codigo == "41000");
        
        informacionEmpresa.CuentaComprasPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "600".PadRight(paddingCC, '0'));
        informacionEmpresa.CuentaVentasPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "700".PadRight(paddingCC, '0'));
        informacionEmpresa.CuentaCobrosPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "572".PadRight(paddingCC, '0'));
        informacionEmpresa.CuentaPagosPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "572".PadRight(paddingCC, '0'));
        
        informacionEmpresa.DiarioVentasPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Ventas");
        informacionEmpresa.DiarioVentasSimplificadasPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Ventas Simplificadas");
        informacionEmpresa.DiarioComprasPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Compras");
        informacionEmpresa.DiarioTesoreriaPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Tesorería");
        informacionEmpresa.DiarioOperacionesVariasPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Operaciones Varias");
        informacionEmpresa.DiarioAperturaPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Apertura");
        informacionEmpresa.DiarioCierrePorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Cierre");
        informacionEmpresa.DiarioRegularizacionPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Regularización");
        informacionEmpresa.PosicionFiscalPorDefecto ??= OS.FirstOrDefault<PosicionFiscal>(p => p.Nombre == "Régimen Nacional");
        informacionEmpresa.ZonaHorariaPorDefecto ??= OS.FirstOrDefault<ZonaHoraria>(z => z.IdZonaHoraria == "Europe/Madrid");

        informacionEmpresa.MedioPagoPorDefecto ??= OS.FirstOrDefault<MedioPago>(m => m.Nombre == "Efectivo");
        informacionEmpresa.CondicionPagoPorDefecto ??= OS.FirstOrDefault<CondicionPago>(c => c.Nombre == "Contado");

        // --- IMPUESTOS POR DEFECTO ---
        if (informacionEmpresa.ImpuestosVentas.Count == 0)
        {
            var iva21Rep = OS.FirstOrDefault<TipoImpuesto>(t => t.Codigo == "IVA21_REP");
            if (iva21Rep != null)
            {
                informacionEmpresa.ImpuestosVentas.Add(iva21Rep);
            }
        }

        if (informacionEmpresa.ImpuestosCompras.Count == 0)
        {
            var iva21Sop = OS.FirstOrDefault<TipoImpuesto>(t => t.Codigo == "IVA21_SOP");
            if (iva21Sop != null)
            {
                informacionEmpresa.ImpuestosCompras.Add(iva21Sop);
            }
        }

        informacionEmpresa.UnidadFacturacionPredeterminada ??= OS.FirstOrDefault<UnidadFacturacion>(u => u.Nombre == "Unidad");

        OS.CommitChanges(); // Nos aseguramos de guardar la empresa inicial para evitar nulos en otras partes si es necesario
        
        InitializeAllSequences(informacionEmpresa);
    }

    private void CreateInitialUnidadesFacturacion()
    {
        var unidades = new[]
        {
            (Nombre: "Unidad", Abreviatura: "ud"),
            (Nombre: "Hora", Abreviatura: "h"),
            (Nombre: "Quilo", Abreviatura: "kg"),
            (Nombre: "Metro", Abreviatura: "m"),
            (Nombre: "Litro", Abreviatura: "l"),
            (Nombre: "Paquete", Abreviatura: "paq"),
            (Nombre: "Caja", Abreviatura: "caja"),
            (Nombre: "Mes", Abreviatura: "mes"),
            (Nombre: "Día", Abreviatura: "día")
        };

        foreach (var u in unidades)
        {
            var unit = OS.FirstOrDefault<UnidadFacturacion>(x => x.Nombre == u.Nombre);
            if (unit == null)
            {
                unit = OS.CreateObject<UnidadFacturacion>();
                unit.Nombre = u.Nombre;
                unit.Abreviatura = u.Abreviatura;
            }
        }
        OS.CommitChanges();
    }

    private void InitializeAllSequences(InformacionEmpresa companyInfo)
    {
        if (OS is not XPObjectSpace xpOs) return;
        var session = xpOs.Session;
        var padding = companyInfo.PaddingNumero;
        var anio = companyInfo.GetLocalTime().Year;
        var mes = companyInfo.GetLocalTime().Month.ToString("D2");

        // Tipos de documentos y sus prefijos
        var docTypes = new (Type Type, string Prefix, string Serie)[]
        {
            (typeof(OfertaVenta), companyInfo.PrefijoOfertasVentaPorDefecto ?? "", companyInfo.PrefijoOfertasVentaPorDefecto ?? ""),
            (typeof(PedidoVenta), companyInfo.PrefijoPedidosPorDefecto ?? "", companyInfo.PrefijoPedidosPorDefecto ?? ""),
            (typeof(AlbaranVenta), companyInfo.PrefijoAlbaranesVentaPorDefecto ?? "", companyInfo.PrefijoAlbaranesVentaPorDefecto ?? ""),
            (typeof(FacturaVenta), companyInfo.PrefijoFacturasVentaPorDefecto ?? "", companyInfo.PrefijoFacturasVentaPorDefecto ?? ""),
            (typeof(FacturaSimplificada), companyInfo.PrefijoFacturasSimplificadasPorDefecto ?? "", companyInfo.PrefijoFacturasSimplificadasPorDefecto ?? ""),
            (typeof(OfertaCompra), companyInfo.PrefijoOfertasCompraPorDefecto ?? "", companyInfo.PrefijoOfertasCompraPorDefecto ?? ""),
            (typeof(PedidoCompra), companyInfo.PrefijoPedidosCompraPorDefecto ?? "", companyInfo.PrefijoPedidosCompraPorDefecto ?? ""),
            (typeof(AlbaranCompra), companyInfo.PrefijoAlbaranesCompraPorDefecto ?? "", companyInfo.PrefijoAlbaranesCompraPorDefecto ?? ""),
            (typeof(FacturaCompra), companyInfo.PrefijoFacturasCompraPorDefecto ?? "", companyInfo.PrefijoFacturasCompraPorDefecto ?? ""),
            (typeof(ParteTrabajo), companyInfo.PrefijoParteTrabajoPorDefecto ?? "", companyInfo.PrefijoParteTrabajoPorDefecto ?? ""),
        };

        foreach (var (type, prefix, serie) in docTypes)
        {
            if (string.IsNullOrEmpty(prefix)) continue;

            var sequenceName = companyInfo.TipoNumeracionDocumento switch
            {
                TipoNumeracionDocumento.PrefijoNumero => $"{type.FullName}.{serie}",
                TipoNumeracionDocumento.PrefijoEjercicioNumero => $"{type.FullName}.{anio}.{serie}",
                TipoNumeracionDocumento.PrefijoEjercicioMesNumero => $"{type.FullName}.{anio}.{mes}.{serie}",
                _ => $"{type.FullName}.{anio}.{serie}"
            };

            SequenceFactory.EnsureSequenceExists(session, sequenceName, prefix, padding);
        }

        // Contactos (estos suelen ser Prefijo/Número siempre según Contacto.cs)
        var contactTypes = new (Type Type, string Prefix)[]
        {
            (typeof(Cliente), companyInfo.PrefijoClientes ?? ""),
            (typeof(Proveedor), companyInfo.PrefijoProveedores ?? ""),
            (typeof(Acreedor), companyInfo.PrefijoAcreedores ?? ""),
            (typeof(Empleado), companyInfo.PrefijoEmpleados ?? ""),
        };

        foreach (var (type, prefix) in contactTypes)
        {
            if (string.IsNullOrEmpty(prefix)) continue;

            var sequenceName = type.FullName ?? type.Name;

            SequenceFactory.EnsureSequenceExists(session, sequenceName, prefix, padding);
        }

        // TPV (Requieren código de TPV, inicializamos para un TPV '01' por defecto si existe o genérico)
        var tpv = OS.FirstOrDefault<BusinessObjects.Tpv.Tpv>(t => true);
        var tpvCodigo = tpv?.Codigo ?? "01";
        
        var tpvTypes = new (Type Type, string Prefix)[]
        {
            (typeof(SesionTpv), companyInfo.PrefijoSesionTpvPorDefecto ?? ""),
            (typeof(VentaTpv), companyInfo.PrefijoVentaTpvPorDefecto ?? ""),
        };

        foreach (var (type, prefix) in tpvTypes)
        {
            if (string.IsNullOrEmpty(prefix)) continue;
            
            var sequenceName = companyInfo.TipoNumeracionDocumento switch
            {
                TipoNumeracionDocumento.PrefijoNumero => $"{type.FullName}.{tpvCodigo}",
                TipoNumeracionDocumento.PrefijoEjercicioNumero => $"{type.FullName}.{anio}.{tpvCodigo}",
                TipoNumeracionDocumento.PrefijoEjercicioMesNumero => $"{type.FullName}.{anio}.{mes}.{tpvCodigo}",
                _ => $"{type.FullName}.{anio}.{tpvCodigo}"
            };
            
            var fullPrefix = $"{prefix}/{tpvCodigo}";
            SequenceFactory.EnsureSequenceExists(session, sequenceName, fullPrefix, padding);
        }

        // Reservas
        if (!string.IsNullOrEmpty(companyInfo.PrefijoReservas))
        {
            var sequenceName = $"erp.Module.BusinessObjects.Alquileres.Reserva.{anio}";
            var prefix = $"{companyInfo.PrefijoReservas}/{anio}";
            SequenceFactory.EnsureSequenceExists(session, sequenceName, prefix, padding);
        }
    }
}