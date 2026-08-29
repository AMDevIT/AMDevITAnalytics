using AMDevIT.Analytics.Abstractions;

namespace AMDevIT.Analytics.Core;

public interface IAnalyticsInstance
{
    #region Methods

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task LogEventAsync(AnalyticsEvent analyticsEvent,
                       CancellationToken cancellationToken = default);

    Task LogEventAsync(string eventID,
                       string? message = null,
                       IReadOnlyDictionary<string, object?>? parameters = null,
                       CancellationToken cancellationToken = default);

    Task LogErrorAsync(CrashEvent crashEvent,
                       CancellationToken cancellationToken = default);

    Task LogErrorAsync(Exception exception,
                       string eventID,
                       string? message = null,
                       IReadOnlyDictionary<string, object?>? parameters = null,
                       CancellationToken cancellationToken = default);

    #endregion
}
