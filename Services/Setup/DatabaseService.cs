using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Microsoft.Data.SqlClient;
using MySqlConnector;

namespace erp.Module.Services.Setup;

public interface IDatabaseService
{
    bool DatabaseExists(string provider, string databaseName);
    void CreateDatabase(string provider, string databaseName);
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

    public void CreateDatabase(string provider, string databaseName)
    {
        var hostConnectionString = configuration.GetConnectionString("ConnectionString");
        if (string.IsNullOrEmpty(hostConnectionString))
            throw new Exception("Cadena de conexión del host no encontrada.");

        switch (provider.ToLower())
        {
            case "postgres":
                CreatePostgresDatabase(hostConnectionString, databaseName);
                break;
            case "mssqlserver":
                CreateMsSqlDatabase(hostConnectionString, databaseName);
                break;
            case "mysql":
                CreateMySqlDatabase(hostConnectionString, databaseName);
                break;
            default:
                CreatePostgresDatabase(hostConnectionString, databaseName);
                break;
        }
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
            var builder = new NpgsqlConnectionStringBuilder(cleanConnectionString);
            builder.Database = databaseName; 

            using var conn = new NpgsqlConnection(builder.ConnectionString);
            conn.Open();
            return true;
        }
        catch (PostgresException ex)
        {
            // 3D000 = invalid_catalog_name (Database does not exist)
            if (ex.SqlState == "3D000")
                return false;
            throw;
        }
        catch (Exception)
        {
            throw; 
        }
    }

    private void CreatePostgresDatabase(string connectionString, string databaseName)
    {
        var cleanConnectionString = CleanConnectionString(connectionString);
        var builder = new NpgsqlConnectionStringBuilder(cleanConnectionString);
        
        // Intentar conectar a 'postgres' por defecto si el usuario tiene permisos
        // pero si no, intentamos sin especificar base de datos (conecta a la DB del usuario)
        try 
        {
            builder.Database = "postgres";
            using var conn = new NpgsqlConnection(builder.ConnectionString);
            conn.Open();
            ExecuteCreatePostgres(conn, databaseName);
        }
        catch (Exception)
        {
            // Fallback: intentar con la conexión original (probablemente DB asignada al usuario)
            builder.Database = ""; 
            using var conn = new NpgsqlConnection(builder.ConnectionString);
            conn.Open();
            ExecuteCreatePostgres(conn, databaseName);
        }
    }

    private void ExecuteCreatePostgres(NpgsqlConnection conn, string databaseName)
    {
        var cmdText = $"CREATE DATABASE \"{databaseName.ToLower()}\"";
        using var cmd = new NpgsqlCommand(cmdText, conn);
        cmd.ExecuteNonQuery();
    }

    private bool CheckMsSqlDatabase(string connectionString, string databaseName)
    {
        try
        {
            var cleanConnectionString = CleanConnectionString(connectionString);
            var builder = new SqlConnectionStringBuilder(cleanConnectionString);
            builder.InitialCatalog = databaseName;

            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            return true;
        }
        catch (SqlException ex)
        {
            // Error 4060: Cannot open database requested by the login
            if (ex.Number == 4060)
                return false;
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void CreateMsSqlDatabase(string connectionString, string databaseName)
    {
        var cleanConnectionString = CleanConnectionString(connectionString);
        var builder = new SqlConnectionStringBuilder(cleanConnectionString);
        
        try 
        {
            builder.InitialCatalog = "master";
            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            ExecuteCreateMsSql(conn, databaseName);
        }
        catch (Exception)
        {
            builder.InitialCatalog = ""; 
            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            ExecuteCreateMsSql(conn, databaseName);
        }
    }

    private void ExecuteCreateMsSql(SqlConnection conn, string databaseName)
    {
        var cmdText = $"CREATE DATABASE [{databaseName}]";
        using var cmd = new SqlCommand(cmdText, conn);
        cmd.ExecuteNonQuery();
    }

    private bool CheckMySqlDatabase(string connectionString, string databaseName)
    {
        try
        {
            var cleanConnectionString = CleanConnectionString(connectionString);
            var builder = new MySqlConnectionStringBuilder(cleanConnectionString);
            builder.Database = databaseName;

            using var conn = new MySqlConnection(builder.ConnectionString);
            conn.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            // Error 1049: Unknown database
            if (ex.Number == 1049)
                return false;
            
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void CreateMySqlDatabase(string connectionString, string databaseName)
    {
        var cleanConnectionString = CleanConnectionString(connectionString);
        var builder = new MySqlConnectionStringBuilder(cleanConnectionString);
        
        // En producción (Plesk), el usuario ya tiene su base de datos asignada.
        // Intentar crearla puede dar error si no es root.
        // Intentamos conectarnos a 'mysql' o 'information_schema' solo si es necesario,
        // pero lo más seguro es intentar conectarse a la base de datos predeterminada del usuario
        // y ejecutar el CREATE DATABASE.
        
        // Si no se especifica base de datos, MySqlConnector suele usar la del usuario o ninguna.
        builder.Database = ""; 

        try 
        {
            using var conn = new MySqlConnection(builder.ConnectionString);
            conn.Open();

            var cmdText = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`";
            using var cmd = new MySqlCommand(cmdText, conn);
            cmd.ExecuteNonQuery();
        }
        catch (MySqlException ex)
        {
            // Si el error es de acceso denegado (1044, 1045) o falta de privilegios (1142), 
            // y la base de datos ya existe (lo cual verificamos antes), podemos ignorarlo.
            // Pero como CheckMySqlDatabase ya se llamó antes en TenantsController, 
            // si llegamos aquí es porque CheckMySqlDatabase devolvió false.
            throw new Exception($"No se pudo crear la base de datos '{databaseName}'. " +
                                $"Asegúrese de que el usuario tiene permisos o que la base de datos ha sido pre-provisionada. " +
                                $"Error: {ex.Message}", ex);
        }
    }

}
