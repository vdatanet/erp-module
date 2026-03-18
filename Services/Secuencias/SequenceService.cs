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

    public int GetNextSequence(string sequenceName, string prefix, int padding, out string formattedSequence)
    {
        const int maxRetries = 5;
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            using var uow = new UnitOfWork(session.DataLayer);
            try
            {
                var generator = uow.FindObject<Secuencia>(new BinaryOperator(nameof(Secuencia.Nombre), sequenceName));
                if (generator == null)
                    generator = new Secuencia(uow)
                    {
                        Nombre = sequenceName,
                        ValorActual = 0,
                        Prefijo = prefix,
                        Relleno = padding
                    };

                generator.ValorActual++;
                uow.CommitChanges();
                formattedSequence = BuildSequenceString(generator);
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
        // XPO suele lanzar excepciones específicas o envolverlas.
        return ex.Message.Contains("duplicate key") || ex.Message.Contains("unique constraint") ||
               ex.InnerException?.Message.Contains("duplicate key") == true ||
               ex.InnerException?.Message.Contains("unique constraint") == true;
    }

    private static string BuildSequenceString(Secuencia generator)
    {
        var number = generator.ValorActual.ToString().PadLeft(generator.Relleno, '0');
        return $"{generator.Prefijo}/{number}";
    }
}