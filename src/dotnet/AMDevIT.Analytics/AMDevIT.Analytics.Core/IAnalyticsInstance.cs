namespace AMDevIT.Analytics.Core;

public interface IAnalyticsInstance
{
    #region Properties

    //ICrashEventLogger CrashEventLogger
    //{
    //    get;
    //}

    //IAnalyticsEventLogger AnalyticsEventLogger
    //{
    //    get;
    //}

    #endregion

    #region Methods

    public Task LogEventAsync(string eventID, string message, CancellationToken cancellationToken = default);
    public Task LogErrorAsync(Exception exception, string eventID, string message, CancellationToken cancellationToken = default);

    #endregion
}
