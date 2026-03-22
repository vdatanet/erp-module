using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Impuestos;
using VeriFactu.Xml.Factu;

namespace erp.Module.Services.Setup;

public class ImpuestoSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialImpuestos()
    {
        if (objectSpace.GetObjectsCount(typeof(TipoImpuesto), null) > 0) return;

        CreateTipoImpuesto("IVA21", "IVA 21%", 21, true, true, Impuesto.IVA);
        CreateTipoImpuesto("IVA10", "IVA 10%", 10, true, true, Impuesto.IVA);
        CreateTipoImpuesto("IVA4", "IVA 4%", 4, true, true, Impuesto.IVA);
        CreateTipoImpuesto("EXENTO", "Exento", 0, true, true, Impuesto.IVA);
    }

    private void CreateTipoImpuesto(string codigo, string nombre, decimal tipo, bool enVentas, bool enCompras,
        Impuesto? impuestoVeriFactu)
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
        tipoImpuesto.EstaActivo = true;
    }
}