namespace AMDevIT.Analytics.Abstractions;

public interface IAnalyticsLoggerSource
    : IAnalyticsSource
{
    #region Methods

    /// <summary>Writes an analytics event to the source.</summary>
    /// <param name="analyticsEvent">Event to write.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task LogEventAsync(AnalyticsEvent analyticsEvent,
                       CancellationToken cancellationToken = default);

    #endregion
}
