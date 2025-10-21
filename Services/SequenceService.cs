using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;
using erp.Module.BusinessObjects.Common;

namespace erp.Module.Services;

public class SequenceService(Session session)
{
    public string GetNextSequence(string sequenceName, string prefix, int padding)
    {
        int maxRetries = 3;
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            using var uow = new UnitOfWork(session.DataLayer);
            var generator = uow.Query<Sequence>().FirstOrDefault(s => s.Name == sequenceName) ?? new Sequence(uow)
            {
                Name = sequenceName,
                CurrentValue = 0,
                Prefix = prefix,
                Padding = padding
            };

            generator.CurrentValue++;
            try
            {
                uow.CommitChanges();
                return BuildSequenceString(generator);
            }
            catch (LockingException)
            {
                if (attempt == maxRetries - 1)
                    throw;
                    
                Thread.Sleep(50); // Espera antes de reintentar
            }
        }
        throw new Exception("No se pudo obtener la secuencia por concurrencia.");
    }

    private static string BuildSequenceString(Sequence generator)
    {
        var number = generator.CurrentValue.ToString().PadLeft(generator.Padding, '0');
        return $"{generator.Prefix}-{number}";
    }
}