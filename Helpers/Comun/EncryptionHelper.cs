using System.Security.Cryptography;
using System.Text;

namespace erp.Module.Helpers.Comun;

public static class EncryptionHelper
{
    private static byte[] GetEncryptionKey()
    {
        // El MachineId no es adecuado porque varía entre clientes y servidor.
        // Usamos una clave de sistema compartida para asegurar que todos los clientes
        // que conectan a la misma base de datos puedan descifrar los valores.
        const string systemSeed = "erp-system-shared-key-2024-v1";
        byte[] salt = "erp-system-salt-2024"u8.ToArray();
        return Rfc2898DeriveBytes.Pbkdf2(systemSeed, salt, 10000, HashAlgorithmName.SHA256, 32);
    }

    private static byte[] GetLegacyMachineKey()
    {
        // Mantener acceso a la clave basada en MachineId para intentar descifrar 
        // datos que fueron cifrados con la versión anterior.
        string machineId = MachineIdentifier.GetMachineId();
        byte[] salt = "erp-system-salt-2024"u8.ToArray();
        return Rfc2898DeriveBytes.Pbkdf2(machineId, salt, 10000, HashAlgorithmName.SHA256, 32);
    }

    public static string? Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        byte[] key = GetEncryptionKey();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        byte[] iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(iv, 0, iv.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string? Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        try
        {
            return DecryptWithKey(cipherText, GetEncryptionKey());
        }
        catch
        {
            try
            {
                // Intentamos con la clave legacy (MachineId)
                return DecryptWithKey(cipherText, GetLegacyMachineKey());
            }
            catch
            {
                // Si falla el descifrado, es posible que el texto esté en plano
                // Retornamos el original para permitir fallback o detección de error.
                return cipherText;
            }
        }
    }

    private static string? DecryptWithKey(string cipherText, byte[] key)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);
        byte[] iv = new byte[16];
        byte[] cipher = new byte[fullCipher.Length - 16];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
        Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
