using System.Security.Cryptography;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;
using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.Services.Secuencias;

public class SequenceService(Session session)
{
    private static int GetJitter(int min, int max)
    {
        return RandomNumberGenerator.GetInt32(min, max);
    }

    public int GetNextSequence(string sequenceName, string prefix, int padding, out string formattedSequence,
        InformacionEmpresa? companyInfo = null, DateTime? fecha = null)
    {
        return GetSequence(sequenceName, prefix, padding, out formattedSequence, true, companyInfo, fecha);
    }

    public void EnsureSequenceExists(string sequenceName, string prefix, int padding)
    {
        GetSequence(sequenceName, prefix, padding, out _, false);
    }

    private int GetSequence(string sequenceName, string prefix, int padding, out string formattedSequence,
        bool increment, InformacionEmpresa? companyInfo = null, DateTime? fecha = null)
    {
        const int maxRetries = 5;
        formattedSequence = string.Empty;
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            using var uow = new UnitOfWork(session.DataLayer);
            try
            {
                uow.LockingOption = LockingOption.Optimistic;
                var generator = uow.FindObject<Secuencia>(PersistentCriteriaEvaluationBehavior.BeforeTransaction,
                    new BinaryOperator(nameof(Secuencia.Nombre), sequenceName));
                if (generator == null)
                    generator = new Secuencia(uow)
                    {
                        Nombre = sequenceName,
                        ValorActual = 0,
                        Prefijo = prefix,
                        Relleno = padding
                    };

                if (increment)
                {
                    generator.ValorActual++;
                }

                if (generator.Prefijo != prefix) generator.Prefijo = prefix;
                if (generator.Relleno != padding) generator.Relleno = padding;

                uow.CommitChanges();
                formattedSequence = companyInfo != null
                    ? BuildSequenceString(generator, companyInfo, fecha)
                    : BuildSequenceString(generator);
                return generator.ValorActual;
            }
            catch (LockingException)
            {
                if (attempt == maxRetries - 1)
                    throw;

                // Espera con jitter para evitar colisiones repetitivas
                Thread.Sleep(50 + GetJitter(10, 50));
            }
            catch (Exception ex) when (IsUniqueConstraintViolation(ex))
            {
                // Manejar colisión si dos hilos intentan crear la misma secuencia simultáneamente
                if (attempt == maxRetries - 1)
                    throw;

                Thread.Sleep(20 + GetJitter(5, 20));
            }
            catch (Exception)
            {
                if (attempt == maxRetries - 1)
                    throw;

                Thread.Sleep(50 + GetJitter(10, 50));
            }
        }

        throw new InvalidOperationException(
            "No se pudo obtener la secuencia por concurrencia tras múltiples reintentos.");
    }

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        // Dependiendo de la DB (Postgres, SQL Server, etc.), el mensaje o el tipo de excepción varía.
        // XPO suele lanzar excepciones específicas como ConstraintViolationException.
        if (ex is ConstraintViolationException || ex.InnerException is ConstraintViolationException)
            return true;

        var message = ex.Message.ToLower();
        var innerMessage = ex.InnerException?.Message.ToLower() ?? "";

        return message.Contains("duplicate key") || message.Contains("unique constraint") ||
               message.Contains("violation of primary key") ||
               innerMessage.Contains("duplicate key") || innerMessage.Contains("unique constraint") ||
               innerMessage.Contains("violation of primary key");
    }

    private static string BuildSequenceString(Secuencia generator)
    {
        var number = generator.ValorActual.ToString().PadLeft(generator.Relleno, '0');
        return $"{generator.Prefijo}/{number}";
    }

    private static string BuildSequenceString(Secuencia generator, InformacionEmpresa companyInfo, DateTime? fecha)
    {
        var number = generator.ValorActual.ToString().PadLeft(generator.Relleno, '0');
        var f = fecha ?? DateTime.Now;

        return companyInfo.TipoNumeracionDocumento switch
        {
            TipoNumeracionDocumento.PrefijoNumero => $"{generator.Prefijo}/{number}",
            TipoNumeracionDocumento.PrefijoEjercicioNumero => $"{generator.Prefijo}/{f.Year}/{number}",
            TipoNumeracionDocumento.PrefijoEjercicioMesNumero => $"{generator.Prefijo}/{f.Year}/{f.Month:D2}/{number}",
            _ => $"{generator.Prefijo}/{number}"
        };
    }
}