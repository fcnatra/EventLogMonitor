namespace InformationMonitor;

public interface IInfoReader
{
    List<Info> GetInformationSince(DateTime moment);
}