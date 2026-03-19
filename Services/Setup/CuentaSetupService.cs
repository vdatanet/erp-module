using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Contabilidad;

namespace erp.Module.Services.Setup;

public class CuentaSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialCuentas()
    {
        CreateIfMissingCuenta("1000", "Capital Social", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateIfMissingCuenta("4300", "Clientes", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateIfMissingCuenta("5720", "Bancos", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateIfMissingCuenta("6000", "Compras", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateIfMissingCuenta("7000", "Ventas", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora, true);
    }

    private void CreateIfMissingCuenta(string codigo, string nombre, Cuenta.TipoCuenta tipo, Cuenta.NaturalezaCuenta naturaleza,
        bool esAsentable = false)
    {
        var cuenta = objectSpace.FirstOrDefault<Cuenta>(c => c.Codigo == codigo);
        if (cuenta == null)
        {
            cuenta = objectSpace.CreateObject<Cuenta>();
            cuenta.Codigo = codigo;
            cuenta.Nombre = nombre;
            cuenta.Tipo = tipo;
            cuenta.Naturaleza = naturaleza;
            cuenta.EsAsentable = esAsentable;
            cuenta.EstaActiva = true;
        }
    }
}