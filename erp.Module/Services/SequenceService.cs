using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Common;

namespace erp.Module.Services;

public class SequenceService(IObjectSpace objectSpace)
{
    public string GetNextSequence(string sequenceName, string prefix, int padding)
    {
        var generator = objectSpace.FirstOrDefault<Sequence>(s => s.Name == sequenceName);

        if (generator == null)
        {
            generator = objectSpace.CreateObject<Sequence>();
            generator.Name = sequenceName;
            generator.CurrentValue = 0;
            generator.Prefix = prefix;
            generator.Padding = padding;
        }

        generator.CurrentValue++;
        var sequence = BuildSequenceString(generator);
        return sequence;
    }

    private static string BuildSequenceString(Sequence generator)
    {
        var number = generator.CurrentValue.ToString().PadLeft(generator.Padding, '0');
        return $"{generator.Prefix}/{number}";
    }
}