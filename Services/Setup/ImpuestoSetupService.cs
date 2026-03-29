using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Base.Facturacion;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.Services.Setup;

public class ImpuestoSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            return compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(TipoImpuesto))) ?? objectSpace;
        }

        return objectSpace;
    }

    public void CreateInitialImpuestos()
    {
        if (!OS.IsKnownType(typeof(TipoImpuesto))) return;
        
        // --- IVA REPERCUTIDO (Ventas S1, Régimen 01) ---
        CreateTipoImpuesto("IVA21_REP", "IVA 21% (Repercutido)", 21, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S1, null, false, "4772100000");
        CreateTipoImpuesto("IVA10_REP", "IVA 10% (Repercutido)", 10, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S1, null, false, "4771000000");
        CreateTipoImpuesto("IVA4_REP", "IVA 4% (Repercutido)", 4, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S1, null, false, "4770400000");
        CreateTipoImpuesto("IVA0_REP", "IVA 0% (Repercutido)", 0, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S1, null, false, "4770000000");

        // --- RECARGO DE EQUIVALENCIA REPERCUTIDO ---
        CreateTipoImpuesto("RE52_REP", "RE 5,2% (Repercutido)", 5.2m, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S1, null, false, "4775200000");
        CreateTipoImpuesto("RE14_REP", "RE 1,4% (Repercutido)", 1.4m, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S1, null, false, "4771400000");
        CreateTipoImpuesto("RE05_REP", "RE 0,5% (Repercutido)", 0.5m, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S1, null, false, "4770500000");

        // --- IVA SOPORTADO (Sin VeriFactu) ---
        CreateTipoImpuesto("IVA21_SOP", "IVA 21% (Soportado)", 21, false, true, null, null, null, null, false, "4722100000");
        CreateTipoImpuesto("IVA10_SOP", "IVA 10% (Soportado)", 10, false, true, null, null, null, null, false, "4721000000");
        CreateTipoImpuesto("IVA4_SOP", "IVA 4% (Soportado)", 4, false, true, null, null, null, null, false, "4720400000");
        CreateTipoImpuesto("IVA0_SOP", "IVA 0% (Soportado)", 0, false, true, null, null, null, null, false, "4720000000");

        // --- RECARGO DE EQUIVALENCIA SOPORTADO ---
        CreateTipoImpuesto("RE52_SOP", "RE 5,2% (Soportado)", 5.2m, false, true, null, null, null, null, false, "4725200000");
        CreateTipoImpuesto("RE14_SOP", "RE 1,4% (Soportado)", 1.4m, false, true, null, null, null, null, false, "4721400000");
        CreateTipoImpuesto("RE05_SOP", "RE 0,5% (Soportado)", 0.5m, false, true, null, null, null, null, false, "4720500000");

        // --- IRPF (RETENCIONES PRACTICADAS - Pasivo) ---
        CreateTipoImpuesto("IRPF15_PRACT", "IRPF Profesionales 15% (Practicada)", 15, true, true, null, null, null, null, true, "4751500000");
        CreateTipoImpuesto("IRPF7_PRACT", "IRPF Nuevos Profesionales 7% (Practicada)", 7, true, true, null, null, null, null, true, "4750700000");
        CreateTipoImpuesto("IRPF19_PRACT", "IRPF Alquileres 19% (Practicada)", 19, true, true, null, null, null, null, true, "4751900000");

        // --- IRPF (RETENCIONES SOPORTADAS - Activo) ---
        CreateTipoImpuesto("IRPF15_SOP", "IRPF Profesionales 15% (Soportada)", 15, true, true, null, null, null, null, true, "4731500000");
        CreateTipoImpuesto("IRPF7_SOP", "IRPF Nuevos Profesionales 7% (Soportada)", 7, true, true, null, null, null, null, true, "4730700000");
        CreateTipoImpuesto("IRPF19_SOP", "IRPF Alquileres 19% (Soportada)", 19, true, true, null, null, null, null, true, "4731900000");

        // --- EXENTOS REPERCUTIDOS (S2) ---
        // E1: Exenta por el art. 20
        CreateTipoImpuesto("IVA_EXENTO_E1", "IVA Exento (Art. 20) (Repercutido)", 0, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S2, CausaExencionAmigable.E1, false, "4770000000");
        // E6: Otros
        CreateTipoImpuesto("IVA_EXENTO_E6", "IVA Exento (Otros) (Repercutido)", 0, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S2, CausaExencionAmigable.E6, false, "4770000000");

        // --- INVERSIÓN SUJETO PASIVO REPERCUTIDO (S3) ---
        CreateTipoImpuesto("IVA_ISP", "IVA Inversión Sujeto Pasivo (Repercutido)", 0, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S3, null, false, "4770000000");

        // --- OTROS REGÍMENES REPERCUTIDOS ---
        // Operaciones Intracomunitarias (Ventas de bienes suelen ser exentas en origen E5)
        CreateTipoImpuesto("IVA_INTRA", "IVA Intracomunitario (Repercutido)", 0, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S2, CausaExencionAmigable.E5, false, "4770000000");
        // Exportaciones (E2)
        CreateTipoImpuesto("IVA_EXPORT", "Exportaciones (Repercutido)", 0, true, false, TipoImpuestoAmigable.IVA, (ClaveRegimen)1, TipoOperacionAmigable.S2, CausaExencionAmigable.E2, false, "4770000000");

        CreateInitialPosicionesFiscales();
    }

    public void CreateInitialPosicionesFiscales()
    {
        CreatePosicionFiscal("Régimen Nacional");
        CreatePosicionFiscal("Intracomunitario");
        CreatePosicionFiscal("Exportación");
        CreatePosicionFiscal("Canarias, Ceuta y Melilla");
        CreatePosicionFiscal("Sujeto Pasivo");
    }

    private void CreatePosicionFiscal(string nombre)
    {
        var posicionFiscal = OS.FirstOrDefault<PosicionFiscal>(p => p.Nombre == nombre);
        if (posicionFiscal == null)
        {
            posicionFiscal = OS.CreateObject<PosicionFiscal>();
            posicionFiscal.Nombre = nombre;
        }

        if (nombre == "Intracomunitario")
        {
            AddMapeo(posicionFiscal, "IVA21_REP", "IVA_INTRA");
            AddMapeo(posicionFiscal, "IVA10_REP", "IVA_INTRA");
            AddMapeo(posicionFiscal, "IVA4_REP", "IVA_INTRA");

            // Mapeos de Cuentas
            AddMapeoCuenta(posicionFiscal, "4300000000", "4300400000"); // Clientes
            AddMapeoCuenta(posicionFiscal, "4000000000", "4000400000"); // Proveedores
            AddMapeoCuenta(posicionFiscal, "4100000000", "4100400000"); // Acreedores
            AddMapeoCuenta(posicionFiscal, "7000000000", "7000400000"); // Ventas
            AddMapeoCuenta(posicionFiscal, "6000000000", "6000400000"); // Compras
        }
        else if (nombre == "Exportación")
        {
            AddMapeo(posicionFiscal, "IVA21_REP", "IVA_EXPORT");
            AddMapeo(posicionFiscal, "IVA10_REP", "IVA_EXPORT");
            AddMapeo(posicionFiscal, "IVA4_REP", "IVA_EXPORT");

            // Mapeos de Cuentas
            AddMapeoCuenta(posicionFiscal, "4300000000", "4300900000"); // Clientes
            AddMapeoCuenta(posicionFiscal, "4000000000", "4000900000"); // Proveedores
            AddMapeoCuenta(posicionFiscal, "7000000000", "7000900000"); // Ventas
            AddMapeoCuenta(posicionFiscal, "6000000000", "6000900000"); // Compras
        }
        else if (nombre == "Canarias, Ceuta y Melilla")
        {
            AddMapeo(posicionFiscal, "IVA21_REP", "IVA_EXPORT");
            AddMapeo(posicionFiscal, "IVA10_REP", "IVA_EXPORT");
            AddMapeo(posicionFiscal, "IVA4_REP", "IVA_EXPORT");

            // Cuentas similares a exportación para fuera de península/baleares
            AddMapeoCuenta(posicionFiscal, "4300000000", "4300900000");
            AddMapeoCuenta(posicionFiscal, "4000000000", "4000900000");
            AddMapeoCuenta(posicionFiscal, "7000000000", "7000900000");
            AddMapeoCuenta(posicionFiscal, "6000000000", "6000900000");
        }
        else if (nombre == "Sujeto Pasivo")
        {
            AddMapeo(posicionFiscal, "IVA21_REP", "IVA_ISP");
            AddMapeo(posicionFiscal, "IVA10_REP", "IVA_ISP");
            AddMapeo(posicionFiscal, "IVA4_REP", "IVA_ISP");
        }
    }

    private void AddMapeoCuenta(PosicionFiscal pf, string origenCod, string destinoCod)
    {
        var origen = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == origenCod);
        var destino = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == destinoCod);

        if (origen != null && destino != null)
        {
            var mapeo = pf.MapeosCuenta.FirstOrDefault(m => m.CuentaOrigen == origen);
            if (mapeo == null)
            {
                mapeo = OS.CreateObject<MapeoCuenta>();
                mapeo.PosicionFiscal = pf;
                mapeo.CuentaOrigen = origen;
            }
            mapeo.CuentaDestino = destino;
        }
    }

    private void AddMapeo(PosicionFiscal pf, string origenCod, string destinoCod)
    {
        var origen = OS.FirstOrDefault<TipoImpuesto>(t => t.Codigo == origenCod);
        var destino = OS.FirstOrDefault<TipoImpuesto>(t => t.Codigo == destinoCod);

        if (origen != null && destino != null)
        {
            var mapeo = pf.Mapeos.FirstOrDefault(m => m.ImpuestoOrigen == origen);
            if (mapeo == null)
            {
                mapeo = OS.CreateObject<MapeoImpuesto>();
                mapeo.PosicionFiscal = pf;
                mapeo.ImpuestoOrigen = origen;
            }
            if (!mapeo.ImpuestosDestino.Contains(destino))
            {
                mapeo.ImpuestosDestino.Add(destino);
            }
        }
    }

    private void CreateTipoImpuesto(string codigo, string nombre, decimal tipo, bool enVentas, bool enCompras,
        TipoImpuestoAmigable? impuestoVeriFactu, ClaveRegimen? regimenFiscal = null, TipoOperacionAmigable? tipoOperacion = null,
        CausaExencionAmigable? causaExencion = null, bool esRetencion = false, string? codigoCuenta = null)
    {
        var tipoImpuesto = OS.FirstOrDefault<TipoImpuesto>(t => t.Codigo == codigo);
        if (tipoImpuesto == null)
        {
            tipoImpuesto = OS.CreateObject<TipoImpuesto>();
            tipoImpuesto.Codigo = codigo;
        }

        tipoImpuesto.Nombre = nombre;
        tipoImpuesto.Tipo = tipo;
        tipoImpuesto.DisponibleEnVentas = enVentas;
        tipoImpuesto.DisponibleEnCompras = enCompras;
        tipoImpuesto.Impuesto = impuestoVeriFactu;
        tipoImpuesto.RegimenFiscal = regimenFiscal;
        tipoImpuesto.TipoOperacion = tipoOperacion;
        tipoImpuesto.CausaExencion = causaExencion;
        tipoImpuesto.EsRetencion = esRetencion;
        tipoImpuesto.EstaActivo = true;

        if (!string.IsNullOrEmpty(codigoCuenta))
        {
            tipoImpuesto.CuentaContable = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == codigoCuenta);
        }
    }
}