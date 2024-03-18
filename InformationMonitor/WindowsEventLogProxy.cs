using System.Diagnostics;
using System.Runtime.Versioning;
namespace InformationMonitor;

[SupportedOSPlatform("windows")]
public class WindowsEventLogProxy() : IInformationProxy
{
    public List<Info> GetAllEventsSince(DateTime moment)
    {
        string logName = "Application";

        EventLog eventLog = new (logName);

        List<Info> events = eventLog.Entries
            .Cast<EventLogEntry>()
            .Where(x => x.TimeWritten >= moment)
            .Select(e => this.ParseEventLogEntry(e)).ToList();

        return events;
    }

    private Info ParseEventLogEntry(EventLogEntry e)
    {
        var info = new Info();
        switch (e.EntryType)
        {
            case EventLogEntryType.Error:
            case EventLogEntryType.FailureAudit:
                info.Level = Definitions.ReportedLevel.Error;
                break;
            case EventLogEntryType.Warning:
                info.Level = Definitions.ReportedLevel.Warning;
                break;
            default:
                info.Level = Definitions.ReportedLevel.Normal;
                break;
        }

        return info;
    }
}