using System.Text.RegularExpressions;

namespace erp.Module.Helpers.Comun;

public static class IdentificacionHelper
{
    public static bool ValidarNif(string? nif)
    {
        if (string.IsNullOrWhiteSpace(nif)) return true;

        nif = nif.ToUpper().Replace("-", "").Replace(" ", "");

        if (nif.Length != 9) return false;

        if (Regex.IsMatch(nif, @"^[0-9]{8}[A-Z]$"))
        {
            return ValidarDNI(nif);
        }

        if (Regex.IsMatch(nif, @"^[XYZ][0-9]{7}[A-Z]$"))
        {
            return ValidarNIE(nif);
        }

        if (Regex.IsMatch(nif, @"^[ABCDEFGHJNPQRSUVW][0-9]{7}[A-Z0-9]$"))
        {
            return ValidarCIF(nif);
        }

        return false;
    }

    private static bool ValidarDNI(string dni)
    {
        string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
        int numero = int.Parse(dni.Substring(0, 8));
        return dni[8] == letras[numero % 23];
    }

    private static bool ValidarNIE(string nie)
    {
        string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
        string nieModificado = nie.Replace("X", "0").Replace("Y", "1").Replace("Z", "2");
        int numero = int.Parse(nieModificado.Substring(0, 8));
        return nie[8] == letras[numero % 23];
    }

    private static bool ValidarCIF(string cif)
    {
        try
        {
            string letras = "ABCDEFGHJNPQRSUVW";
            if (!letras.Contains(cif[0])) return false;

            int sumaPares = 0;
            for (int i = 2; i <= 6; i += 2)
            {
                sumaPares += int.Parse(cif[i].ToString());
            }

            int sumaImpares = 0;
            for (int i = 1; i <= 7; i += 2)
            {
                int doble = int.Parse(cif[i].ToString()) * 2;
                sumaImpares += (doble / 10) + (doble % 10);
            }

            int sumaTotal = sumaPares + sumaImpares;
            int numControl = (10 - (sumaTotal % 10)) % 10;
            char[] letrasControl = { 'J', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' };

            char ultimoCaracter = cif[8];

            if (char.IsDigit(ultimoCaracter))
            {
                return int.Parse(ultimoCaracter.ToString()) == numControl;
            }
            else
            {
                return ultimoCaracter == letrasControl[numControl];
            }
        }
        catch
        {
            return false;
        }
    }
}
