namespace AMDevIT.Analytics.Abstractions;

public interface ICrashEventLoggerSource
    : IAnalyticsSource
{
    #region Methods

    Task LogErrorAsync(CrashEvent crashEvent,
                       CancellationToken cancellationToken = default);

    #endregion
}
