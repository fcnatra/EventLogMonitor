using System.Diagnostics;
using InformationMonitor;

namespace RunningProcessesInformationSource;

public class ProcessesReader : IInformationSourceProxy
{
    public List<Info> GetAllEventsSince(DateTime moment)
    {
        var processes = Process.GetProcesses();
        var infoFilteredAndParsed = processes
            .Where(p => p.StartTime >= moment)
            .Select(x => ParseProcessInformation(x))
            .ToList();

        return infoFilteredAndParsed;
    }

    private static Info ParseProcessInformation(Process x)
    {
        var info = new Info();
        
        switch(x.PriorityClass)
        {
            case ProcessPriorityClass.High:
            case ProcessPriorityClass.AboveNormal:
                info.Level = Definitions.ReportedLevel.Error;
                break;
            
            case ProcessPriorityClass.RealTime:
            case ProcessPriorityClass.BelowNormal:
                info.Level = Definitions.ReportedLevel.Warning;
                break;

            default:
                info.Level = Definitions.ReportedLevel.Normal;
                break;
        }
        
        info.Moment = x.StartTime;
        info.Information = x.ProcessName;
        return info;
    }
}
