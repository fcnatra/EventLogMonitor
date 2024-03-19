
namespace InformationMonitor;

public class InfoReader : IInfoReader
{
    private IInformationSourceProxy sourceProxy;

    public InfoReader(IInformationSourceProxy informationSourceProxy)
    {
        this.sourceProxy = informationSourceProxy;
    }

    public List<Info> GetInformationSince(DateTime moment)
    {
        List<Info> entries;

        if (DateTime.Now.Subtract(moment) < new TimeSpan(0, 0, 1))
            return [];

        try
        {
            entries = this.sourceProxy
                .GetAllEventsSince(moment)
                .Where(e => e.Level != Definitions.ReportedLevel.Normal)
                .ToList();            
        }
        catch (System.Exception)
        {
            entries = [];
        }

        return entries;
    }
}
