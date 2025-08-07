using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Common;
using Task = System.Threading.Tasks.Task;

namespace erp.Module.Services;

public class SequenceService(IObjectSpace objectSpace)
{
    private async Task<string> GetNextSequenceAsync(string sequenceName, string prefix, int padding)
    {
        int maxRetries = 3;
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            var session = ((XPObjectSpace)objectSpace).Session;
            using var uow = new UnitOfWork(session.DataLayer);
            var generator = uow.Query<Sequence>().FirstOrDefault(s => s.Name == sequenceName);

            if (generator == null)
            {
                generator = new Sequence(uow)
                {
                    Name = sequenceName,
                    CurrentValue = 0,
                    Prefix = prefix,
                    Padding = padding
                };
            }

            generator.CurrentValue++;
            try
            {
                await uow.CommitChangesAsync();
                return BuildSequenceString(generator);
            }
            catch (DevExpress.Xpo.DB.Exceptions.LockingException)
            {
                // Otro proceso modificó la fila, reintenta
                if (attempt == maxRetries - 1)
                    throw; // Si ya reintentaste varias veces, lanza la excepción
                await Task.Delay(50); // Espera un poco antes de reintentar
            }
        }
        throw new Exception("No se pudo obtener la secuencia por concurrencia.");
    }

    // Versión síncrona para compatibilidad
    public string GetNextSequence(string sequenceName, string prefix, int padding)
    {
        return GetNextSequenceAsync(sequenceName, prefix, padding).GetAwaiter().GetResult();
    }

    private static string BuildSequenceString(Sequence generator)
    {
        var number = generator.CurrentValue.ToString().PadLeft(generator.Padding, '0');
        return $"{generator.Prefix}/{number}";
    }
}