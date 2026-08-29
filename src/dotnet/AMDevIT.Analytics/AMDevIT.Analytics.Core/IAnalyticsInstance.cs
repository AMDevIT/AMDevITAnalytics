using AMDevIT.Analytics.Abstractions;

namespace AMDevIT.Analytics.Core;

public interface IAnalyticsInstance
{
    #region Methods

    /// <summary>Initializes all registered analytics sources.</summary>
    /// <param name="cancellationToken">Token used to cancel initialization.</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Writes an analytics event to all registered analytics sources.</summary>
    /// <param name="analyticsEvent">Event to write.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task LogEventAsync(AnalyticsEvent analyticsEvent,
                       CancellationToken cancellationToken = default);

    /// <summary>Creates and writes an analytics event to all registered analytics sources.</summary>
    /// <param name="eventID">Event identifier.</param>
    /// <param name="message">Optional event message.</param>
    /// <param name="parameters">Optional event parameters.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task LogEventAsync(string eventID,
                       string? message = null,
                       IReadOnlyDictionary<string, object?>? parameters = null,
                       CancellationToken cancellationToken = default);

    /// <summary>Writes a crash event to all registered crash sources.</summary>
    /// <param name="crashEvent">Crash event to write.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task LogErrorAsync(CrashEvent crashEvent,
                       CancellationToken cancellationToken = default);

    /// <summary>Creates and writes a crash event to all registered crash sources.</summary>
    /// <param name="exception">Exception associated with the crash.</param>
    /// <param name="eventID">Event identifier.</param>
    /// <param name="message">Optional event message.</param>
    /// <param name="parameters">Optional event parameters.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task LogErrorAsync(Exception exception,
                       string eventID,
                       string? message = null,
                       IReadOnlyDictionary<string, object?>? parameters = null,
                       CancellationToken cancellationToken = default);

    #endregion
}
