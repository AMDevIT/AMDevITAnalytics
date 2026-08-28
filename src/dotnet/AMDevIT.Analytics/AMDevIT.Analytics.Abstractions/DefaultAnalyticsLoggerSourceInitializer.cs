namespace AMDevIT.Analytics.Abstractions;

public sealed class DefaultAnalyticsLoggerSourceInitializer
    : IAnalyticsLoggerSourceInitializer
{
    #region Properties

    public bool IsInitialized
    {
        get;
        private set;
    }

    #endregion

    #region Methods

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Just do nothing.
        this.IsInitialized = true;
        return Task.CompletedTask;
    }

    #endregion
}
