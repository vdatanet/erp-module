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
            builder.Database = "postgres"; 

            using var conn = new NpgsqlConnection(builder.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName", conn);
            cmd.Parameters.AddWithValue("dbName", databaseName.ToLower());
            var result = cmd.ExecuteScalar();
            return result != null;
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
        builder.Database = "postgres";

        using var conn = new NpgsqlConnection(builder.ConnectionString);
        conn.Open();

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
            builder.InitialCatalog = "master";

            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand("SELECT 1 FROM sys.databases WHERE name = @dbName", conn);
            cmd.Parameters.AddWithValue("dbName", databaseName);
            var result = cmd.ExecuteScalar();
            return result != null;
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
        builder.InitialCatalog = "master";

        using var conn = new SqlConnection(builder.ConnectionString);
        conn.Open();

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
            builder.Database = "information_schema";

            using var conn = new MySqlConnection(builder.ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand("SELECT 1 FROM SCHEMATA WHERE SCHEMA_NAME = @dbName", conn);
            cmd.Parameters.AddWithValue("dbName", databaseName);
            var result = cmd.ExecuteScalar();
            return result != null;
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
        builder.Database = "mysql";

        using var conn = new MySqlConnection(builder.ConnectionString);
        conn.Open();

        var cmdText = $"CREATE DATABASE `{databaseName}`";
        using var cmd = new MySqlCommand(cmdText, conn);
        cmd.ExecuteNonQuery();
    }

}
