using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Contabilidad;

namespace erp.Module.Services.Setup;

public class CuentaSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            var result = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(CuentaContable)));
            if (result != null) return result;

            // Fallback to the first persistent Object Space if no specific match is found for the type
            var fallback = compositeOS.AdditionalObjectSpaces.FirstOrDefault();
            if (fallback != null) return fallback;
        }

        return objectSpace;
    }

    public void CreateInitialCuentas()
    {
        CreatePgcEspanol();
    }

    private void CreatePgcEspanol()
    {
        // GRUPO 1: FINANCIACIÓN BÁSICA
        CreateCuenta("1", "Financiación básica", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("10", "Capital", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("100", "Capital social", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("11", "Reservas", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("112", "Reserva legal", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("12", "Resultados pendientes de aplicación", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("120", "Remanente", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("121", "Resultados negativos de ejercicios anteriores", CuentaContable.TipoCuenta.PatrimonioNeto, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("129", "Resultado del ejercicio", CuentaContable.TipoCuenta.Resultados, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("17", "Deudas a largo plazo por préstamos recibidos, empréstitos y otros conceptos", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("170", "Deudas a largo plazo con entidades de crédito", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);

        // GRUPO 2: INMOVILIZADO
        CreateCuenta("2", "Inmovilizado", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("20", "Inmovilizaciones intangibles", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("206", "Aplicaciones informáticas", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("21", "Inmovilizaciones materiales", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("210", "Terrenos y bienes naturales", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("211", "Construcciones", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("213", "Maquinaria", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("216", "Mobiliario", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("217", "Equipos para procesos de información", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("218", "Elementos de transporte", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);

        // GRUPO 3: EXISTENCIAS
        CreateCuenta("3", "Existencias", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("30", "Comerciales", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("300", "Mercaderías A", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);

        // GRUPO 4: ACREEDORES Y DEUDORES POR OPERACIONES COMERCIALES
        CreateCuenta("4", "Acreedores y deudores por operaciones comerciales", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("40", "Proveedores", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("400", "Proveedores", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("40000", "Proveedores (euros) - Nacionales", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("41", "Acreedores varios", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("410", "Acreedores por prestaciones de servicios", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("41000", "Acreedores por prestaciones de servicios (euros)", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("43", "Clientes", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("430", "Clientes", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4300", "Clientes (euros)", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("43000", "Clientes (euros) - Nacionales", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("44", "Deudores varios", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("440", "Deudores", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4400", "Deudores (euros)", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("44000", "Deudores (euros) - Nacionales", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("46", "Personal", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("460", "Anticipos de remuneraciones", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("465", "Remuneraciones pendientes de pago", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("47", "Administraciones Públicas", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("472", "HP IVA soportado", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("47221", "HP IVA soportado 21%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4722100000", "HP IVA soportado 21%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47210", "HP IVA soportado 10%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4721000000", "HP IVA soportado 10%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47204", "HP IVA soportado 4%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4720400000", "HP IVA soportado 4%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47200", "HP IVA soportado 0%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4720000000", "HP IVA soportado 0%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47252", "HP RE soportado 5,2%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4725200000", "HP RE soportado 5,2%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47214", "HP RE soportado 1,4%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4721400000", "HP RE soportado 1,4%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47205", "HP RE soportado 0,5%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4720500000", "HP RE soportado 0,5%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("473", "HP Retenciones y pagos a cuenta", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("47315", "HP Retenciones Soportadas Prof. 15%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4731500000", "HP Retenciones Soportadas Prof. 15%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47307", "HP Retenciones Soportadas Prof. 7%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4730700000", "HP Retenciones Soportadas Prof. 7%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47319", "HP Retenciones Soportadas Alquileres 19%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("4731900000", "HP Retenciones Soportadas Alquileres 19%", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("475", "Hacienda Pública, acreedora por diversos conceptos", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("47515", "HP acreedora Retenciones IRPF 15%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4751500000", "HP acreedora Retenciones IRPF 15%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47507", "HP acreedora Retenciones IRPF 7%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4750700000", "HP acreedora Retenciones IRPF 7%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47519", "HP acreedora Retenciones IRPF 19%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4751900000", "HP acreedora Retenciones IRPF 19%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("476", "Organismos de la Seguridad Social, acreedores", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("477", "HP IVA repercutido", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("47721", "HP IVA repercutido 21%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4772100000", "HP IVA repercutido 21%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47710", "HP IVA repercutido 10%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4771000000", "HP IVA repercutido 10%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47704", "HP IVA repercutido 4%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4770400000", "HP IVA repercutido 4%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47700", "HP IVA repercutido 0%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4770000000", "HP IVA repercutido 0%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47752", "HP RE repercutido 5,2%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4775200000", "HP RE repercutido 5,2%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47714", "HP RE repercutido 1,4%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4771400000", "HP RE repercutido 1,4%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47705", "HP RE repercutido 0,5%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("4770500000", "HP RE repercutido 0,5%", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora, true);

        // GRUPO 5: CUENTAS FINANCIERAS
        CreateCuenta("5", "Cuentas financieras", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("52", "Deudas a corto plazo por préstamos recibidos y otros conceptos", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("520", "Deudas a corto plazo con entidades de crédito", CuentaContable.TipoCuenta.Pasivo, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("57", "Tesorería", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("570", "Caja, euros", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("572", "Bancos e instituciones de crédito c/c a la vista, euros", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("57200", "Bancos e instituciones de crédito c/c - CuentaContable principal", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("5720000000", "Bancos e instituciones de crédito c/c - CuentaContable principal", CuentaContable.TipoCuenta.Activo, CuentaContable.NaturalezaCuenta.Deudora, true);

        // GRUPO 6: COMPRAS Y GASTOS
        CreateCuenta("6", "Compras y gastos", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("60", "Compras", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("600", "Compras de mercaderías", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("60000", "Compras de mercaderías - Nacionales", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("6000000000", "Compras de mercaderías - Nacionales", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora, true);
        CreateCuenta("62", "Servicios exteriores", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("621", "Arrendamientos y cánones", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("622", "Reparaciones y conservación", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("623", "Servicios de profesionales independientes", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("624", "Transportes", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("625", "Primas de seguros", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("626", "Servicios bancarios y similares", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("627", "Publicidad, propaganda y relaciones públicas", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("628", "Suministros", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("629", "Otros servicios", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("64", "Gastos de personal", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("640", "Sueldos y salarios", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);
        CreateCuenta("642", "Seguridad Social a cargo de la empresa", CuentaContable.TipoCuenta.Gastos, CuentaContable.NaturalezaCuenta.Deudora);

        // GRUPO 7: VENTAS E INGRESOS
        CreateCuenta("7", "Ventas e ingresos", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("70", "Ventas de mercaderías, de producción propia, de servicios, etc.", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("700", "Ventas de mercaderías", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("70000", "Ventas de mercaderías - Nacionales", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("7000000000", "Ventas de mercaderías - Nacionales", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("705", "Prestaciones de servicios", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("70500", "Prestaciones de servicios (euros)", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("7050000000", "Prestaciones de servicios (euros)", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("75", "Otros ingresos de gestión", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("752", "Ingresos por arrendamientos", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
        CreateCuenta("759", "Ingresos por servicios diversos", CuentaContable.TipoCuenta.Ingresos, CuentaContable.NaturalezaCuenta.Acreedora);
    }

    private void CreateCuenta(string codigo, string nombre, CuentaContable.TipoCuenta tipo, CuentaContable.NaturalezaCuenta naturaleza,
        bool esAsentable = false)
    {
        var cuenta = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == codigo);
        if (cuenta == null)
        {
            cuenta = OS.CreateObject<CuentaContable>();
            cuenta.Codigo = codigo;
        }

        cuenta.Nombre = nombre;
        cuenta.Tipo = tipo;
        cuenta.Naturaleza = naturaleza;
        cuenta.EsAsentable = esAsentable;
        cuenta.EstaActiva = true;
    }
}