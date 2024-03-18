using InformationMonitor;
using FakeItEasy;

namespace Tests;

public class GivenReader
{
    private IInformationProxy fakeEventLogProxy;
    private InfoReader reader;
    private DateTime NowMinus(TimeSpan timeSpan) => DateTime.Now.Subtract(timeSpan);
    private readonly TimeSpan TenMinutes = new TimeSpan(0, 10, 0);
    private readonly TimeSpan FiveMinutes = new TimeSpan(0, 5, 0);
    private readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
    private readonly TimeSpan TenSeconds = new TimeSpan(0, 0, 10);
    private readonly TimeSpan FiveSeconds = new TimeSpan(0, 0, 5);
    private readonly TimeSpan OneSecond = new TimeSpan(0, 0, 1);
    private readonly TimeSpan HalfASecond = new TimeSpan(0, 0, 0, 0, 500);

    public GivenReader()
    {
        this.fakeEventLogProxy = A.Fake<IInformationProxy>();
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
    public void WhenTimeIsLessThan1SecondReturnsEmptyList()
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
        IEnumerable<Info> eventList = this.reader.GetEventsFrom(DateTime.Now.Subtract(TenSeconds)).ToList();

        // Then
        Assert.Empty(eventList);
    }

    [Fact]
    public void WhenThereAreEventsOnlyReturnsEventsOutOfNormal()
    {
        // Given
        A.CallTo(() => fakeEventLogProxy.GetAllEventsSince(A<DateTime>._)).Returns(
            [
                new() { Moment = NowMinus(FiveSeconds), Level = Definitions.ReportedLevel.Normal },
                new() { Moment = NowMinus(FiveSeconds), Level = Definitions.ReportedLevel.Warning },
                new() { Moment = NowMinus(FiveSeconds), Level = Definitions.ReportedLevel.Error }
            ]
        );

        // When
        List<Info> eventList = reader.GetEventsFrom(NowMinus(TenMinutes)).ToList();

        // Then
        Assert.DoesNotContain(eventList, (e) => e.Level == Definitions.ReportedLevel.Normal);
        Assert.Equal(2, eventList.Count((e) => e.Level != Definitions.ReportedLevel.Normal));
    }

    [Fact]
    public void WhenErrorInProxyReturnsEmptyList()
    {
        // Given
        A.CallTo(() => fakeEventLogProxy.GetAllEventsSince(A<DateTime>._)).Throws<Exception>();

        // When
        List<Info> eventList = reader.GetEventsFrom(NowMinus(OneMinute)).ToList();

        // Then
        Assert.Empty(eventList);
    }
}
