namespace InformationMonitor;

public class InfoController
{
    private DateTime NowMinus(TimeSpan timeSpan) => DateTime.Now.Subtract(timeSpan);
    private readonly TimeSpan TenMinutes = new TimeSpan(0, 10, 0);
    private Timer? continuousMonitoring;
    private int oneSecondInMs = 1000;

    public void MonitorLastTenMinutes(IInfoReader reader, IInfoDispatcher infoDispatcher)
    {
        List<Info> infoEntries = reader.GetInformationSince(NowMinus(TenMinutes));
        infoDispatcher.Dispatch(infoEntries);
    }

    /// <summary>
    /// Runs a continuous monitoring every second
    /// </summary>
    /// <exception cref="InvalidOperationException">If previous monitoring is runnning</exception>
    public void MonitorEverySecond(IInfoReader reader, IInfoDispatcher dispatcher)
    {
        if (this.continuousMonitoring is not null)
            throw new InvalidOperationException("Continuous monitoring is running. Stop it before running another.");

        var state = (reader, dispatcher);
        this.continuousMonitoring = new Timer(this.ContinuousMonitoringCallback, state, 0, oneSecondInMs);
    }

    private void ContinuousMonitoringCallback(object? state)
    {
        if (state is null)
        {
            this.Stop();
            return;
        }

        (IInfoReader reader, IInfoDispatcher dispatcher) = (ValueTuple<IInfoReader, IInfoDispatcher>)state;

        try
        {
            List<Info> infoEntries = reader.GetInformationSince(NowMinus(TenMinutes));
            dispatcher.Dispatch(infoEntries);       
        }
        catch
        {
            this.Stop();
        }    
    }

    public void Stop()
    {
        this.continuousMonitoring?.Dispose();
        this.continuousMonitoring = null;
    }

    public bool IsContinuousMonitoringRunning => this.continuousMonitoring == null;
}
