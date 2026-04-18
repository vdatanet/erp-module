using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl.MultiTenancy;
using erp.Module.BusinessObjects;

namespace erp.Module.Services.Setup;

public class TenantSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            var result = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(Tenant)));
            if (result != null) return result;

            // Fallback to the first persistent Object Space if no specific match is found for the type
            var fallback = compositeOS.AdditionalObjectSpaces.FirstOrDefault();
            if (fallback != null) return fallback;
        }

        return objectSpace;
    }

    public Tenant CreateTenant(string tenantName, string databaseName, string provider, string server, string user, string password)
    {
        var tenant = OS.FirstOrDefault<Tenant>(t => t.Name == tenantName);
        if (tenant == null)
        {
            tenant = OS.CreateObject<Tenant>();
            tenant.Name = tenantName;

            var connectionString = provider.ToLower() switch
            {
                "postgres" =>
                    $"XpoProvider=Postgres;Server={server};User ID={user};Password={password};database={databaseName}",
                "mssqlserver" =>
                    $"XpoProvider=MSSqlServer;data source={server};user id={user};password={password};initial catalog={databaseName};TrustServerCertificate=True",
                "mysql" => $"XpoProvider=MySql;Server={server};User ID={user};Password={password};database={databaseName}",
                _ => throw new NotSupportedException($"Proveedor de base de datos no soportado: {provider}")
            };

            tenant.ConnectionString = connectionString;
        }

        return tenant;
    }

    public void CreateInitialTenants(string? currentTenantName)
    {
#if DEBUG
        if (currentTenantName == null)
        {
            _ = CreateTenant("demo", "erp_demo", "Postgres", "db-local", "joan", "");
            _ = CreateTenant("demo-mssql", "erp_demo_mssql", "MSSqlServer", ".\\SQLEXPRESS", "sa", "your_password");
            _ = CreateTenant("demo-mysql", "erp_demo_mysql", "MySql", "db-local", "devuser", "");
        }
#endif
    }
}