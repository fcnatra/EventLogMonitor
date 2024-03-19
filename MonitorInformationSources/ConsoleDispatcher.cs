using InformationMonitor;

namespace MonitorInformationSources;

class ConsoleDispatcher(Func<Info, string> Parse) : IInfoDispatcher
{
    public void Dispatch(List<Info> infoEntries)
    {
        foreach(var entry in infoEntries)
            Console.WriteLine(Parse(entry));
    }
}