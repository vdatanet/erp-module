using System;
using System.Linq;
using System.Threading;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using Task = System.Threading.Tasks.Task;

namespace erp.Module.Services;

public class SequenceService(Session session)
{
    public string GetNextSequence(string sequenceName, string prefix, int padding)
    {
        int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using (var uow = new UnitOfWork(session.DataLayer))
            {
                var generator = uow.Query<Sequence>().FirstOrDefault(s => s.Name == sequenceName);

                if (generator == null)
                {
                    generator = new Sequence(uow);
                    generator.Name = sequenceName;
                    generator.CurrentValue = 0;
                    generator.Prefix = prefix;
                    generator.Padding = padding;
                }

                generator.CurrentValue++;
                try
                {
                    uow.CommitChanges();
                    return BuildSequenceString(generator);
                }
                catch (DevExpress.Xpo.DB.Exceptions.LockingException)
                {
                    if (attempt == maxRetries - 1)
                        throw;
                    
                    Thread.Sleep(50); // Espera antes de reintentar
                }
            }
        }
        throw new Exception("No se pudo obtener la secuencia por concurrencia.");
    }

    private static string BuildSequenceString(Sequence generator)
    {
        var number = generator.CurrentValue.ToString().PadLeft(generator.Padding, '0');
        return $"{generator.Prefix}/{number}";
    }
}