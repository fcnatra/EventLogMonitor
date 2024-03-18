
namespace InformationMonitor;

public class InfoReader
{
    private IInformationProxy eventLogProxy;

    public InfoReader(IInformationProxy eventLogProxy)
    {
        this.eventLogProxy = eventLogProxy;
    }

    public List<Info> GetEventsFrom(DateTime moment)
    {
        List<Info> entries;

        if (DateTime.Now.Subtract(moment) < new TimeSpan(0, 0, 1))
            return [];

        try
        {
            entries = this.eventLogProxy
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
