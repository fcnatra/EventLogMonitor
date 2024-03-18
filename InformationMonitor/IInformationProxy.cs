
namespace InformationMonitor;

public interface IInformationProxy
{
    List<Info> GetAllEventsSince(DateTime moment);
}