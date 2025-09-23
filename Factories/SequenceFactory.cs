using DevExpress.Xpo;
using erp.Module.Services;

namespace erp.Module.Factories;

public static class SequenceFactory
{
    public static string GetNextSequence(Session session, string sequenceName, string prefix = "", int padding = 6)
    {
        var service = new SequenceService(session);
        return service.GetNextSequence(sequenceName, prefix, padding);
    }
}