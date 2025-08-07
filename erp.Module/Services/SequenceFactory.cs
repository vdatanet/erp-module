using DevExpress.ExpressApp;

namespace erp.Module.Services;

public static class SequenceFactory
{
    public static string GetNextSequence(
        IObjectSpace objectSpace,
        string sequenceName,
        string prefix = "",
        int padding = 6)
    {
        var service = new SequenceService(objectSpace);
        return service.GetNextSequence(sequenceName, prefix, padding);
    }
}