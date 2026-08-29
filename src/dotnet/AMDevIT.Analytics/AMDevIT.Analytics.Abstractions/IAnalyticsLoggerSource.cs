namespace AMDevIT.Analytics.Abstractions;

public interface IAnalyticsLoggerSource
    : IAnalyticsSource
{
    #region Methods

    Task LogEventAsync(AnalyticsEvent analyticsEvent,
                       CancellationToken cancellationToken = default);

    #endregion
}
