using InformationMonitor;
using FakeItEasy;

namespace Tests;

public class GivenController
{
    private IInfoReader fakeInfoReader;
    private IInfoDispatcher fakeDispatcher;
    private InfoController infoController;
    
    public GivenController()
    {
        fakeInfoReader = A.Fake<IInfoReader>();
        fakeDispatcher = A.Fake<IInfoDispatcher>();
        infoController = new();
    }

    [Fact]
    public void RequestLastInfoMustRequestAndDispatchJustOnce()
    {
        // Given
        // --

        // When
        infoController.MonitorLastTenMinutes(fakeInfoReader, fakeDispatcher);

        // Then
        A.CallTo(() => fakeInfoReader.GetInformationSince(A<DateTime>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeDispatcher.Dispatch(A<List<Info>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RequestForContinuousMonitoringIsDoneUnatended()
    {
        // Given
        //..

        // When
        infoController.MonitorEverySecond(fakeInfoReader, fakeDispatcher);

        // Then
        Thread.Sleep(1500);
        infoController.Stop();
        A.CallTo(() => fakeInfoReader.GetInformationSince(A<DateTime>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => fakeDispatcher.Dispatch(A<List<Info>>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public void ContinuousMonitoringSendsInfoToDispatcher()
    {
        // Given
        List<Info> infoEntries = [new Info { Level = Definitions.ReportedLevel.Warning, Moment = DateTime.Now }];

        A.CallTo(() => fakeInfoReader.GetInformationSince(A<DateTime>._))
            .Returns(infoEntries);

        // When
        infoController.MonitorEverySecond(fakeInfoReader, fakeDispatcher);

        // Then
        Thread.Sleep(2500);
        infoController.Stop();
        A.CallTo(() => fakeInfoReader.GetInformationSince(A<DateTime>._)).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => fakeDispatcher.Dispatch(A<List<Info>>.That.IsSameAs(infoEntries))).MustHaveHappened(3, Times.Exactly);
    }

    [Fact]
    public void ContinuousMonitoringCalled2ndTimePriorToStopThrowsException()
    {
        // Given
        List<Info> infoEntries = [new Info { Level = Definitions.ReportedLevel.Warning, Moment = DateTime.Now }];
        infoController.MonitorEverySecond(fakeInfoReader, fakeDispatcher);

        // When && Then
        Assert.Throws<InvalidOperationException>(() => infoController.MonitorEverySecond(fakeInfoReader, fakeDispatcher));
        infoController.Stop();
    }
    
    [Fact]
    public void ContinuousMonitoringStopsIfReaderFails()
    {
        // Given
        List<Info> infoEntries = [new Info { Level = Definitions.ReportedLevel.Warning, Moment = DateTime.Now }];

        // When
        infoController.MonitorEverySecond(fakeInfoReader, fakeDispatcher);

        // Then
        Thread.Sleep(100);
        A.CallTo(() => fakeInfoReader.GetInformationSince(A<DateTime>._)).Throws<NullReferenceException>();
        Thread.Sleep(2000);

        A.CallTo(() => fakeDispatcher.Dispatch(A<List<Info>>.That.IsSameAs(infoEntries))).MustHaveHappenedOnceExactly();
        Assert.False(infoController.IsContinuousMonitoringRunning);
    }
}
