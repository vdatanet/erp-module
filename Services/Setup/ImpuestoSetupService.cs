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
        // --- IVA RÉGIMEN GENERAL (Ventas S1, Régimen 01) ---
        CreateTipoImpuesto("IVA21", "IVA 21%", 21, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47721");
        CreateTipoImpuesto("IVA10", "IVA 10%", 10, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47710");
        CreateTipoImpuesto("IVA4", "IVA 4%", 4, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47704");
        CreateTipoImpuesto("IVA0", "IVA 0%", 0, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47700");

        // --- RECARGO DE EQUIVALENCIA ---
        CreateTipoImpuesto("RE52", "Recargo Equivalencia 5,2%", 5.2m, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47752");
        CreateTipoImpuesto("RE14", "Recargo Equivalencia 1,4%", 1.4m, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47714");
        CreateTipoImpuesto("RE05", "Recargo Equivalencia 0,5%", 0.5m, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47705");

        // --- IRPF (RETENCIONES) ---
        CreateTipoImpuesto("IRPF15", "IRPF Profesionales 15%", 15, true, true, null, null, null, null, true, "47515");
        CreateTipoImpuesto("IRPF7", "IRPF Nuevos Profesionales 7%", 7, true, true, null, null, null, null, true, "47507");
        CreateTipoImpuesto("IRPF19", "IRPF Alquileres 19%", 19, true, true, null, null, null, null, true, "47519");

        // --- EXENTOS (S2) ---
        // E1: Exenta por el art. 20
        CreateTipoImpuesto("IVA_EXENTO_E1", "IVA Exento (Art. 20)", 0, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E1, false, "47700");
        // E6: Otros
        CreateTipoImpuesto("IVA_EXENTO_E6", "IVA Exento (Otros)", 0, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E6, false, "47700");

        // --- INVERSIÓN SUJETO PASIVO (S3) ---
        CreateTipoImpuesto("IVA_ISP", "IVA Inversión Sujeto Pasivo", 0, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S1, null, false, "47700");

        // --- OTROS REGÍMENES ---
        // Operaciones Intracomunitarias (Ventas de bienes suelen ser exentas en origen E5)
        CreateTipoImpuesto("IVA_INTRA", "IVA Intracomunitario", 0, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E5, false, "47700");
        // Exportaciones (E2)
        CreateTipoImpuesto("IVA_EXPORT", "Exportaciones", 0, true, true, Impuesto.IVA, (ClaveRegimen)1, CalificacionOperacion.S2, CausaExencion.E2, false, "47700");
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
            tipoImpuesto.Cuenta = objectSpace.FirstOrDefault<Cuenta>(c => c.Codigo == codigoCuenta);
        }
    }
}