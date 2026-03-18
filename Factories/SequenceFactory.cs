using DevExpress.Xpo;
using erp.Module.Services.Sequences;

namespace erp.Module.Factories;

public static class SequenceFactory
{
    public static string GetNextSequence(Session session, string sequenceName, string prefix = "", int padding = 6)
    {
        var service = new SequenceService(session);
        service.GetNextSequence(sequenceName, prefix, padding, out var formattedSequence);
        return formattedSequence;
    }

    public static int GetNextSequence(Session session, string sequenceName, out string formattedSequence,
        string prefix = "", int padding = 6)
    {
        var service = new SequenceService(session);
        return service.GetNextSequence(sequenceName, prefix, padding, out formattedSequence);
    }
}