using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl.MultiTenancy;

namespace erp.Module.Services.Setup;

public class TenantSetupService(IObjectSpace objectSpace)
{
    public Tenant CreateTenant(string tenantName, string databaseName, string provider = "Postgres")
    {
        var tenant = objectSpace.FirstOrDefault<Tenant>(t => t.Name == tenantName);
        if (tenant == null)
        {
            tenant = objectSpace.CreateObject<Tenant>();
            tenant.Name = tenantName;

            var connectionString = provider switch
            {
                "Postgres" =>
                    $"XpoProvider=Postgres;Server=db-local;User ID=postgres;Password=;database={databaseName}",
                "MSSqlServer" =>
                    $"XpoProvider=MSSqlServer;data source=.\\SQLEXPRESS;integrated security=SSPI;initial catalog={databaseName};TrustServerCertificate=True",
                "MySql" => $"XpoProvider=MySql;Server=db-local;User ID=devuser;Password=;database={databaseName}",
                _ => $"XpoProvider=Postgres;Server=db-local;User ID=postgres;Password=;database={databaseName}"
            };

            tenant.ConnectionString = connectionString;
        }

        return tenant;
    }
}