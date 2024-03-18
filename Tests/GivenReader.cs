namespace Tests;

public class GivenReader
{
    [Fact]
    public void WhenNoLogsReturnEmptyList()
    {
        EventLogMonitor.EventLogReader reader = new();

        var TenSeconds = new TimeSpan(0, 0, 15);

        var eventList = reader.GetEventsFrom(DateTime.Now.Subtract(TenSeconds));

        Assert.Empty(eventList);
    }
}