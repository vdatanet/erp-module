using System;
using System.Security.Cryptography;
using System.Text;

namespace erp.Module.Helpers.Comun
{
    public class GuidHelper
    {
        public static string GetShortHash(Guid guid, int length = 12)
        {
            // 1. Convertim el GUID a bytes
            byte[] guidBytes = guid.ToByteArray();

            // 2. Calculem el Hash SHA-256 per garantir una distribució uniforme
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(guidBytes);

                // 3. Convertim a Base64 (més compacte que Hexadecimal)
                string base64 = Convert.ToBase64String(hashBytes);

                // 4. Fem que sigui "URL Safe" (substituïm caràcters conflictius)
                // i tallem a la longitud desitjada
                return base64.Replace("+", "-")
                             .Replace("/", "_")
                             .Replace("=", "")
                             .Substring(0, length);
            }
        }
    }
}
