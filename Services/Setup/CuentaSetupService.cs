using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Contabilidad;

namespace erp.Module.Services.Setup;

public class CuentaSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialCuentas()
    {
        CreatePgcEspanol();
    }

    private void CreatePgcEspanol()
    {
        // GRUPO 1: FINANCIACIÓN BÁSICA
        CreateCuenta("1", "Financiación básica", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("10", "Capital", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("100", "Capital social", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("11", "Reservas", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("112", "Reserva legal", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("12", "Resultados pendientes de aplicación", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("120", "Remanente", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("121", "Resultados negativos de ejercicios anteriores", Cuenta.TipoCuenta.PatrimonioNeto, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("129", "Resultado del ejercicio", Cuenta.TipoCuenta.Resultados, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("17", "Deudas a largo plazo por préstamos recibidos, empréstitos y otros conceptos", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("170", "Deudas a largo plazo con entidades de crédito", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);

        // GRUPO 2: INMOVILIZADO
        CreateCuenta("2", "Inmovilizado", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("20", "Inmovilizaciones intangibles", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("206", "Aplicaciones informáticas", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("21", "Inmovilizaciones materiales", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("210", "Terrenos y bienes naturales", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("211", "Construcciones", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("213", "Maquinaria", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("216", "Mobiliario", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("217", "Equipos para procesos de información", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("218", "Elementos de transporte", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);

        // GRUPO 3: EXISTENCIAS
        CreateCuenta("3", "Existencias", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("30", "Comerciales", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("300", "Mercaderías A", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);

        // GRUPO 4: ACREEDORES Y DEUDORES POR OPERACIONES COMERCIALES
        CreateCuenta("4", "Acreedores y deudores por operaciones comerciales", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("40", "Proveedores", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("400", "Proveedores", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("40000", "Proveedores (euros) - Nacionales", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("41", "Acreedores varios", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("410", "Acreedores por prestaciones de servicios", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("41000", "Acreedores por prestaciones de servicios (euros)", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("43", "Clientes", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("430", "Clientes", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("4300", "Clientes (euros)", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("43000", "Clientes (euros) - Nacionales", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("44", "Deudores varios", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("440", "Deudores", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("4400", "Deudores (euros)", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("44000", "Deudores (euros) - Nacionales", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("46", "Personal", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("460", "Anticipos de remuneraciones", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("465", "Remuneraciones pendientes de pago", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("47", "Administraciones Públicas", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("472", "HP IVA soportado", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("47221", "HP IVA soportado 21%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47210", "HP IVA soportado 10%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47204", "HP IVA soportado 4%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47200", "HP IVA soportado 0%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47252", "HP RE soportado 5,2%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47214", "HP RE soportado 1,4%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47205", "HP RE soportado 0,5%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("473", "HP Retenciones y pagos a cuenta", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("47315", "HP Retenciones Soportadas Prof. 15%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47307", "HP Retenciones Soportadas Prof. 7%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("47319", "HP Retenciones Soportadas Alquileres 19%", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora, true);
        CreateCuenta("475", "Hacienda Pública, acreedora por diversos conceptos", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("47515", "HP acreedora Retenciones IRPF 15%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47507", "HP acreedora Retenciones IRPF 7%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47519", "HP acreedora Retenciones IRPF 19%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("476", "Organismos de la Seguridad Social, acreedores", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("477", "HP IVA repercutido", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("47721", "HP IVA repercutido 21%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47710", "HP IVA repercutido 10%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47704", "HP IVA repercutido 4%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47700", "HP IVA repercutido 0%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47752", "HP RE repercutido 5,2%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47714", "HP RE repercutido 1,4%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);
        CreateCuenta("47705", "HP RE repercutido 0,5%", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora, true);

        // GRUPO 5: CUENTAS FINANCIERAS
        CreateCuenta("5", "Cuentas financieras", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("52", "Deudas a corto plazo por préstamos recibidos y otros conceptos", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("520", "Deudas a corto plazo con entidades de crédito", Cuenta.TipoCuenta.Pasivo, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("57", "Tesorería", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("570", "Caja, euros", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("572", "Bancos e instituciones de crédito c/c a la vista, euros", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("57200", "Bancos e instituciones de crédito c/c - Cuenta principal", Cuenta.TipoCuenta.Activo, Cuenta.NaturalezaCuenta.Deudora);

        // GRUPO 6: COMPRAS Y GASTOS
        CreateCuenta("6", "Compras y gastos", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("60", "Compras", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("600", "Compras de mercaderías", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("60000", "Compras de mercaderías - Nacionales", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("62", "Servicios exteriores", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("621", "Arrendamientos y cánones", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("622", "Reparaciones y conservación", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("623", "Servicios de profesionales independientes", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("624", "Transportes", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("625", "Primas de seguros", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("626", "Servicios bancarios y similares", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("627", "Publicidad, propaganda y relaciones públicas", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("628", "Suministros", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("629", "Otros servicios", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("64", "Gastos de personal", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("640", "Sueldos y salarios", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);
        CreateCuenta("642", "Seguridad Social a cargo de la empresa", Cuenta.TipoCuenta.Gastos, Cuenta.NaturalezaCuenta.Deudora);

        // GRUPO 7: VENTAS E INGRESOS
        CreateCuenta("7", "Ventas e ingresos", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("70", "Ventas de mercaderías, de producción propia, de servicios, etc.", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("700", "Ventas de mercaderías", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("70000", "Ventas de mercaderías - Nacionales", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("705", "Prestaciones de servicios", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("70500", "Prestaciones de servicios (euros)", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("75", "Otros ingresos de gestión", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("752", "Ingresos por arrendamientos", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
        CreateCuenta("759", "Ingresos por servicios diversos", Cuenta.TipoCuenta.Ingresos, Cuenta.NaturalezaCuenta.Acreedora);
    }

    private void CreateCuenta(string codigo, string nombre, Cuenta.TipoCuenta tipo, Cuenta.NaturalezaCuenta naturaleza,
        bool esAsentable = false)
    {
        var cuenta = objectSpace.FirstOrDefault<Cuenta>(c => c.Codigo == codigo);
        if (cuenta == null)
        {
            cuenta = objectSpace.CreateObject<Cuenta>();
            cuenta.Codigo = codigo;
        }

        cuenta.Nombre = nombre;
        cuenta.Tipo = tipo;
        cuenta.Naturaleza = naturaleza;
        cuenta.EsAsentable = esAsentable;
        cuenta.EstaActiva = true;
    }
}