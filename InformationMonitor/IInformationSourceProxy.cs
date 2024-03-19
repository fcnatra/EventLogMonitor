
namespace InformationMonitor;

public interface IInformationSourceProxy
{
    List<Info> GetAllEventsSince(DateTime moment);
}