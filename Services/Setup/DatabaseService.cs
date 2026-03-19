using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Microsoft.Data.SqlClient;
using MySqlConnector;

namespace erp.Module.Services.Setup;

public interface IDatabaseService
{
    bool DatabaseExists(string provider, string databaseName);
}

public class DatabaseService(IConfiguration configuration) : IDatabaseService
{
    public bool DatabaseExists(string provider, string databaseName)
    {
        var hostConnectionString = configuration.GetConnectionString("ConnectionString");
        if (string.IsNullOrEmpty(hostConnectionString)) return false;

        return provider.ToLower() switch
        {
            "postgres" => CheckPostgresDatabase(hostConnectionString, databaseName),
            "mssqlserver" => CheckMsSqlDatabase(hostConnectionString, databaseName),
            "mysql" => CheckMySqlDatabase(hostConnectionString, databaseName),
            _ => CheckPostgresDatabase(hostConnectionString, databaseName)
        };
    }

    private string CleanConnectionString(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !p.Trim().StartsWith("XpoProvider=", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return string.Join(";", parts);
    }

    private bool CheckPostgresDatabase(string connectionString, string databaseName)
    {
        try
        {
            var cleanConnectionString = CleanConnectionString(connectionString);
            Console.WriteLine($"[DEBUG_LOG] Postgres: cleanConnectionString='{cleanConnectionString}'");
            // Extraer la parte de la conexión sin el nombre de la base de datos para conectar al servidor de administración
            var builder = new NpgsqlConnectionStringBuilder(cleanConnectionString);
            builder.Database = "postgres"; // Conectamos a la DB por defecto del sistema
            
            using var conn = new NpgsqlConnection(builder.ConnectionString);
            conn.Open();
            Console.WriteLine($"[DEBUG_LOG] Postgres: Conectado para comprobar existencia de '{databaseName}'");
            using var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName", conn);
            cmd.Parameters.AddWithValue("dbName", databaseName.ToLower());
            var result = cmd.ExecuteScalar();
            Console.WriteLine($"[DEBUG_LOG] Postgres: Resultado de búsqueda '{databaseName}': {result}");
            return result != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG_LOG] Error comprobando DB Postgres '{databaseName}': {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"[DEBUG_LOG] Inner Error: {ex.InnerException.Message}");
            throw; // Propagamos el error para que el controlador no asuma que no existe si hay un fallo de conexión
        }
    }

    private bool CheckMsSqlDatabase(string connectionString, string databaseName)
    {
        try
        {
            var cleanConnectionString = CleanConnectionString(connectionString);
            var builder = new SqlConnectionStringBuilder(cleanConnectionString);
            builder.InitialCatalog = "master";
            
            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand("SELECT 1 FROM sys.databases WHERE name = @dbName", conn);
            cmd.Parameters.AddWithValue("dbName", databaseName);
            var result = cmd.ExecuteScalar();
            return result != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG_LOG] Error comprobando DB SQL Server '{databaseName}': {ex.Message}");
            throw;
        }
    }

    private bool CheckMySqlDatabase(string connectionString, string databaseName)
    {
        try
        {
            var cleanConnectionString = CleanConnectionString(connectionString);
            var builder = new MySqlConnectionStringBuilder(cleanConnectionString);
            builder.Database = "information_schema";
            
            using var conn = new MySqlConnection(builder.ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand("SELECT 1 FROM SCHEMATA WHERE SCHEMA_NAME = @dbName", conn);
            cmd.Parameters.AddWithValue("dbName", databaseName);
            var result = cmd.ExecuteScalar();
            return result != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG_LOG] Error comprobando DB MySQL '{databaseName}': {ex.Message}");
            throw;
        }
    }
}
