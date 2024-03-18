using EventLogMonitor;
using FakeItEasy;

namespace Tests;

public class GivenReader
{
        private IEventLogProxy fakeEventLogProxy;
        private WindowsEventsReader reader;

    public GivenReader()
    {
        this.fakeEventLogProxy = A.Fake<IEventLogProxy>();
        this.reader = new(fakeEventLogProxy);
    }

    [Fact]
    public void WhenNoLogsReturnsEmptyList()
    {
        // Given
        var TenSeconds = new TimeSpan(0, 0, 15);

        // When
        var eventList = this.reader.GetEventsFrom(DateTime.Now.Subtract(TenSeconds));

        // Then
        Assert.Empty(eventList);
    }

    [Fact]
    public void WhenTimeIsLessThanOneSecondReturnsEmptyList()
    {
        // Given
        var TenSeconds = new TimeSpan(0, 0, 0, 0, 500);
        A.CallTo(() => fakeEventLogProxy.GetAllEventsSince(A<DateTime>._)).Returns(
            [
                new() { Level = Definitions.ReportedLevel.Normal },
                new() { Level = Definitions.ReportedLevel.Warning },
                new() { Level = Definitions.ReportedLevel.Error }
            ]
        );

        // When
        IEnumerable<Event> eventList = this.reader.GetEventsFrom(DateTime.Now.Subtract(TenSeconds)).ToList();

        // Then
        Assert.Empty(eventList);
    }

    [Fact]
    public void WhenThereAreEventsOnlyReturnsEventsWithWarning()
    {
        // Given
        var TenSeconds = new TimeSpan(0, 0, 5);
        A.CallTo(() => fakeEventLogProxy.GetAllEventsSince(A<DateTime>._)).Returns(
            [
                new() { Level = Definitions.ReportedLevel.Normal },
                new() { Level = Definitions.ReportedLevel.Warning },
                new() { Level = Definitions.ReportedLevel.Error }
            ]
        );

        // When
        IEnumerable<Event> eventList = reader.GetEventsFrom(DateTime.Now.Subtract(TenSeconds));

        // Then
        Assert.True(eventList.All((e) => e.Level == Definitions.ReportedLevel.Warning));
        Assert.Equal(1, eventList.Count((e) => e.Level == Definitions.ReportedLevel.Warning));
    }
}
