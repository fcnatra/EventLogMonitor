using System.Runtime.Versioning;
using EventLogInformationSource;
using InformationMonitor;
using RunningProcessesInformationSource;

namespace MonitorInformationSources;

[SupportedOSPlatform("windows")]
class Program
{
    static void Main(string[] args)
    {
        var sourceSelection = "all";

        if (args.Any()) sourceSelection = args[0].ToLower();

        switch(sourceSelection)
        {
            case "event": PrintLastLogs(new EventLogReader(), new ConsoleDispatcher(ParsingActions.ForConsole)); break;
            case "process": PrintLastLogs(new ProcessesReader(), new ConsoleDispatcher(ParsingActions.ForConsole)); break;
            default: PrintLastLogsFromSeveralSources(new List<IInformationSourceProxy>
            {
                new EventLogReader(),
                new ProcessesReader()
            },
            new ConsoleDispatcher(ParsingActions.ForConsole)
            ); break;
        }        
    }

    private static void PrintLastLogsFromSeveralSources(List<IInformationSourceProxy> readingSources, IInfoDispatcher dispatcher)
    {
        var bufferedDispatcher = new NoDispatchButBufferDispatcher();
        readingSources.ForEach(source => {
            var monitorController = new InfoController(new InfoReader(source), bufferedDispatcher);
            monitorController.MonitorLastTenMinutes();
        });

        PrintHeaderIntoDispatcher(dispatcher, $"Last 10 minutes of information from: {string.Join(", ", readingSources.Select(s => s.GetType().Name))}");
        dispatcher.Dispatch(bufferedDispatcher.RecoverEntriesOrderedByTime());
    }

    private static void PrintLastLogs(IInformationSourceProxy readingSource, IInfoDispatcher dispatcher)
    {
        PrintHeaderIntoDispatcher(dispatcher, $"Last 10 minutes of information from: {readingSource.GetType().Name}");

        var monitorController = new InfoController(new InfoReader(readingSource), dispatcher);
        monitorController.MonitorLastTenMinutes();
    }

    private static void PrintHeaderIntoDispatcher(IInfoDispatcher dispatcher, string message)
    {
        dispatcher.Dispatch(new List<Info> {
            new Info {
                 Moment = DateTime.Now,
                 Level = Definitions.ReportedLevel.Normal,
                 Information = message
            }
        });
    }
}

class NoDispatchButBufferDispatcher : IInfoDispatcher
{
    private List<Info> bufferedEntries = new List<Info>();

    public void Dispatch(List<Info> infoEntries)
    {
        this.bufferedEntries.AddRange(infoEntries);
    }

    public List<Info> RecoverEntriesOrderedByTime() =>  bufferedEntries.OrderBy(e => e.Moment).ToList();
}
