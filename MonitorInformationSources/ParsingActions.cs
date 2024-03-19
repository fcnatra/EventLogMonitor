using System.Text;
using InformationMonitor;

namespace MonitorInformationSources;

static class ParsingActions
{
    public static string ForConsole(Info entry)
    {
        var consoleOutput = new StringBuilder(entry.Moment.ToString("yyyy-MM-dd HH:mm:ss  "))
            .Append($"{entry.Level, -15}")
            .Append(entry.Information)
            .ToString();

        return consoleOutput;
    }
}
