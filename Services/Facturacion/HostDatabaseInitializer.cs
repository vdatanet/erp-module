using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace erp.Module.Services.Facturacion;

/// <summary>
/// Inicializa la base de datos del Host para asegurar que las tablas compartidas (como VeriFactuAudit) existen.
/// </summary>
public class HostDatabaseInitializer(IConfiguration configuration, ILogger<HostDatabaseInitializer> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Iniciando HostDatabaseInitializer para actualizar esquema del Host.");

            string? connectionString = configuration.GetConnectionString("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogWarning("No se encontró la cadena de conexión 'ConnectionString' para el Host.");
                return Task.CompletedTask;
            }

            // Creamos un ObjectSpaceProvider minimalista para el Host que solo conozca VeriFactuAudit
            using var provider = new XPObjectSpaceProvider(connectionString, null);
            
            // Registramos el tipo VeriFactuAudit en el diccionario de XPO para este provider
            provider.TypesInfo.RegisterEntity(typeof(VeriFactuAudit));

            using var objectSpace = provider.CreateObjectSpace();
            
            // Esto fuerza la actualización del esquema en la base de datos apuntada (el Host)
            logger.LogInformation("Actualizando esquema en la base de datos del Host...");
            // provider.UpdateDatabase() se encarga de llamar al DatabaseUpdater si existe, 
            // pero XPO crea las tablas automáticamente al inicializarse si el esquema no coincide y se configura así.
            // En XAF, solemos llamar al updater explícitamente o confiar en que provider.CreateObjectSpace() 
            // dispara la inicialización de la capa de datos.
            
            // Para asegurar la creación de tablas en XPO sin el updater completo de XAF:
            var session = ((XPObjectSpace)objectSpace).Session;
            session.UpdateSchema(typeof(VeriFactuAudit));
            session.CreateObjectTypeRecords(typeof(VeriFactuAudit));

            logger.LogInformation("Esquema del Host actualizado correctamente para VeriFactuAudit.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar la base de datos del Host.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
