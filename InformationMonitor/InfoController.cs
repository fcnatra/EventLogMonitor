namespace InformationMonitor;

public class InfoController
{
    private static DateTime NowMinus(TimeSpan timeSpan) => DateTime.Now.Subtract(timeSpan);
	private readonly TimeSpan TenMinutes = new(0, 10, 0);
	private readonly TimeSpan OneSecond = new(0, 0, 1);
	private Timer? continuousMonitoring;

	public IInfoReader Reader { get; set; }

	public IInfoDispatcher Dispatcher { get; set; }

	public InfoController(IInfoReader reader, IInfoDispatcher dispatcher)
    {
        this.Reader = reader;
        this.Dispatcher = dispatcher;
    }

    public void MonitorLastTenMinutes()
    {
        List<Info> infoEntries = Reader.GetInformationSince(NowMinus(TenMinutes));
        Dispatcher.Dispatch(infoEntries);
    }

    /// <summary>
    /// Runs a continuous monitoring every second
    /// </summary>
    /// <exception cref="InvalidOperationException">If previous monitoring is runnning</exception>
    public void MonitorEverySecond()
    {
		int oneSecondInMs = 1000;

		if (this.continuousMonitoring is not null)
            throw new InvalidOperationException("Continuous monitoring is running. Stop it before running another.");

        this.continuousMonitoring = new Timer(this.ContinuousMonitoringCallback, null, 0, oneSecondInMs);
    }

    private void ContinuousMonitoringCallback(object? state)
    {
        try
        {
            List<Info> infoEntries = Reader.GetInformationSince(NowMinus(OneSecond));
            Dispatcher.Dispatch(infoEntries);
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

    public bool IsContinuousMonitoringRunning => this.continuousMonitoring != null;
}
