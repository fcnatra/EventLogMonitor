using System.Diagnostics;
using System.Runtime.Versioning;
using InformationMonitor;

namespace EventLogInformationSource;

[SupportedOSPlatform("windows")]
public class EventLogReader : IInformationSourceProxy
{
    public enum EventLogSectionName {System, Application, Security}

    /// <summary>
    /// Default value: Application
    /// </summary>
    public EventLogSectionName LogName { get; set;} = EventLogSectionName.Application;

    public List<Info> GetAllEventsSince(DateTime moment)
    {
        var eventLogAccesor = new System.Diagnostics.EventLog(LogName.ToString());

        List<Info> events = eventLogAccesor.Entries
            .Cast<EventLogEntry>()
            .Where(x => x.TimeWritten >= moment)
            .Select(e => this.ParseEventLogEntry(e))
            .ToList();

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
        info.Moment = e.TimeWritten;
        info.Information = e.Message;
        return info;
    }
}
