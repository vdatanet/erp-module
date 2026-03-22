using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Impuestos;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.Services.Setup;

public class ImpuestoSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialImpuestos()
    {
        // --- IVA REPERCUTIDO (Ventas S1, Régimen 01) ---
        CreateTipoImpuesto("IVA21_REP", "IVA 21% (Repercutido)", 21, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47721");
        CreateTipoImpuesto("IVA10_REP", "IVA 10% (Repercutido)", 10, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47710");
        CreateTipoImpuesto("IVA4_REP", "IVA 4% (Repercutido)", 4, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47704");
        CreateTipoImpuesto("IVA0_REP", "IVA 0% (Repercutido)", 0, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47700");

        // --- RECARGO DE EQUIVALENCIA REPERCUTIDO ---
        CreateTipoImpuesto("RE52_REP", "RE 5,2% (Repercutido)", 5.2m, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47752");
        CreateTipoImpuesto("RE14_REP", "RE 1,4% (Repercutido)", 1.4m, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47714");
        CreateTipoImpuesto("RE05_REP", "RE 0,5% (Repercutido)", 0.5m, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47705");

        // --- IVA SOPORTADO (Sin VeriFactu) ---
        CreateTipoImpuesto("IVA21_SOP", "IVA 21% (Soportado)", 21, false, true, null, null, null, null, false, "47221");
        CreateTipoImpuesto("IVA10_SOP", "IVA 10% (Soportado)", 10, false, true, null, null, null, null, false, "47210");
        CreateTipoImpuesto("IVA4_SOP", "IVA 4% (Soportado)", 4, false, true, null, null, null, null, false, "47204");
        CreateTipoImpuesto("IVA0_SOP", "IVA 0% (Soportado)", 0, false, true, null, null, null, null, false, "47200");

        // --- RECARGO DE EQUIVALENCIA SOPORTADO ---
        CreateTipoImpuesto("RE52_SOP", "RE 5,2% (Soportado)", 5.2m, false, true, null, null, null, null, false, "47252");
        CreateTipoImpuesto("RE14_SOP", "RE 1,4% (Soportado)", 1.4m, false, true, null, null, null, null, false, "47214");
        CreateTipoImpuesto("RE05_SOP", "RE 0,5% (Soportado)", 0.5m, false, true, null, null, null, null, false, "47205");

        // --- IRPF (RETENCIONES PRACTICADAS - Pasivo) ---
        CreateTipoImpuesto("IRPF15_PRACT", "IRPF Profesionales 15% (Practicada)", 15, true, true, null, null, null, null, true, "47515");
        CreateTipoImpuesto("IRPF7_PRACT", "IRPF Nuevos Profesionales 7% (Practicada)", 7, true, true, null, null, null, null, true, "47507");
        CreateTipoImpuesto("IRPF19_PRACT", "IRPF Alquileres 19% (Practicada)", 19, true, true, null, null, null, null, true, "47519");

        // --- IRPF (RETENCIONES SOPORTADAS - Activo) ---
        CreateTipoImpuesto("IRPF15_SOP", "IRPF Profesionales 15% (Soportada)", 15, true, true, null, null, null, null, true, "47315");
        CreateTipoImpuesto("IRPF7_SOP", "IRPF Nuevos Profesionales 7% (Soportada)", 7, true, true, null, null, null, null, true, "47307");
        CreateTipoImpuesto("IRPF19_SOP", "IRPF Alquileres 19% (Soportada)", 19, true, true, null, null, null, null, true, "47319");

        // --- EXENTOS REPERCUTIDOS (S2) ---
        // E1: Exenta por el art. 20
        CreateTipoImpuesto("IVA_EXENTO_E1", "IVA Exento (Art. 20) (Repercutido)", 0, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E1, false, "47700");
        // E6: Otros
        CreateTipoImpuesto("IVA_EXENTO_E6", "IVA Exento (Otros) (Repercutido)", 0, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E6, false, "47700");

        // --- INVERSIÓN SUJETO PASIVO REPERCUTIDO (S3) ---
        CreateTipoImpuesto("IVA_ISP", "IVA Inversión Sujeto Pasivo (Repercutido)", 0, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47700");

        // --- OTROS REGÍMENES REPERCUTIDOS ---
        // Operaciones Intracomunitarias (Ventas de bienes suelen ser exentas en origen E5)
        CreateTipoImpuesto("IVA_INTRA", "IVA Intracomunitario (Repercutido)", 0, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E5, false, "47700");
        // Exportaciones (E2)
        CreateTipoImpuesto("IVA_EXPORT", "Exportaciones (Repercutido)", 0, true, false, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E2, false, "47700");
    }

    private void CreateTipoImpuesto(string codigo, string nombre, decimal tipo, bool enVentas, bool enCompras,
        Impuesto? impuestoVeriFactu, ClaveRegimen? regimenFiscal = null, CalificacionOperacion? tipoOperacion = null,
        CausaExencion? causaExencion = null, bool esRetencion = false, string? codigoCuenta = null)
    {
        var tipoImpuesto = objectSpace.FirstOrDefault<TipoImpuesto>(t => t.Codigo == codigo);
        if (tipoImpuesto == null)
        {
            tipoImpuesto = objectSpace.CreateObject<TipoImpuesto>();
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
            tipoImpuesto.CuentaContable = objectSpace.FirstOrDefault<CuentaContable>(c => c.Codigo == codigoCuenta);
        }
    }
}