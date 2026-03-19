using System.Security.Cryptography;
using System.Text;

namespace erp.Module.Helpers;

public static class DevExpressPasswordHelper
{
    public static bool VerifyPassword(string hashedPassword, string password)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return string.IsNullOrEmpty(password);
        
        if (string.IsNullOrEmpty(password))
            return false;

        byte[] data;
        try
        {
            data = Convert.FromBase64String(hashedPassword);
        }
        catch (FormatException)
        {
            return false;
        }

        // Verificar formato: 49 bytes total, primer byte = 0
        if (data.Length != 49 || data[0] != 0)
            return false;

        // Extraer salt (bytes 1-16) y hash (bytes 17-48)
        byte[] salt = new byte[16];
        byte[] storedHash = new byte[32];
        Buffer.BlockCopy(data, 1, salt, 0, 16);
        Buffer.BlockCopy(data, 17, storedHash, 0, 32);

        // Generar hash con la contraseña proporcionada
        byte[] computedHash = GenerateHash(password, salt);

        // Comparar hashes
        return AreEqual(storedHash, computedHash);
    }

    private static byte[] GenerateHash(string password, byte[] salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 20000, HashAlgorithmName.SHA1))
        {
            return pbkdf2.GetBytes(32);
        }
    }

    private static bool AreEqual(byte[] x, byte[] y)
    {
        if (x.Length != y.Length)
            return false;

        bool result = true;
        for (int i = 0; i < x.Length; i++)
        {
            result &= x[i] == y[i];
        }
        return result;
    }
    
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return password;

        // Generar salt aleatorio de 16 bytes
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Generar hash
        byte[] hash = GenerateHash(password, salt);

        // Crear el array final: [0][salt][hash]
        byte[] result = new byte[49];
        result[0] = 0; // Versión
        Buffer.BlockCopy(salt, 0, result, 1, 16);
        Buffer.BlockCopy(hash, 0, result, 17, 32);

        return Convert.ToBase64String(result);
    }
}
