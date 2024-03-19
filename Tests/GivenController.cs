using InformationMonitor;
using FakeItEasy;
using System.Diagnostics;

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
        infoController = new(fakeInfoReader, fakeDispatcher);
    }

    [Fact]
    public void RequestLastInfoMustRequestAndDispatchJustOnce()
    {
        // Given
        // --

        // When
        infoController.MonitorLastTenMinutes();

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
        infoController.MonitorEverySecond();

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
        infoController.MonitorEverySecond();

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
        infoController.MonitorEverySecond();

        // When
        void action() => infoController.MonitorEverySecond();

        // Then
        Assert.Throws<InvalidOperationException>(action);
        infoController.Stop();
    }
    
    [Fact]
    public void ContinuousMonitoringStopsIfReaderFails()
    {
        // Given
        List<Info> infoEntries = [new Info { Level = Definitions.ReportedLevel.Warning, Moment = DateTime.Now }];

        A.CallTo(() => fakeInfoReader.GetInformationSince(A<DateTime>._)).Invokes(() => {
            Trace.WriteLine("First time calling the READER ~~~~~~~~~~~~~~~~~~");
        });

        infoController.MonitorEverySecond();
        Thread.Sleep(100);

        // When
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        infoController.Reader = null; // Force Exception
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Then
        Thread.Sleep(2000);
        A.CallTo(() => fakeDispatcher.Dispatch(A<List<Info>>._)).MustHaveHappenedOnceExactly();
        Assert.False(infoController.IsContinuousMonitoringRunning);
    }
}
