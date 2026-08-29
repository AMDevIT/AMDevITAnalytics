namespace AMDevIT.Analytics.Abstractions;

public interface ICrashEventLoggerSource
    : IAnalyticsSource
{
    #region Methods

    /// <summary>Writes a crash event to the source.</summary>
    /// <param name="crashEvent">Crash event to write.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task LogErrorAsync(CrashEvent crashEvent,
                       CancellationToken cancellationToken = default);

    #endregion
}
