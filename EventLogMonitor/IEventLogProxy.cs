
namespace EventLogMonitor;

public interface IEventLogProxy
{
    List<Event> GetAllEventsSince(DateTime moment);
}