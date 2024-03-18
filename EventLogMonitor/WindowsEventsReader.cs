
namespace EventLogMonitor;

public class WindowsEventsReader : IWindowsEventsReader
{
    private IEventLogProxy eventLogProxy;

    public WindowsEventsReader(IEventLogProxy eventLogProxy)
    {
        this.eventLogProxy = eventLogProxy;
    }

    public IEnumerable<Event> GetEventsFrom(DateTime moment)
    {
        if (DateTime.Now.Subtract(moment) < new TimeSpan(0, 0, 1))
            return [];

        List<Event> events = this.eventLogProxy.GetAllEventsSince(moment);

        return events.Where(e => e.Level == Definitions.ReportedLevel.Warning);
    }
}
