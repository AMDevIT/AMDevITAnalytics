namespace AMDevIT.Analytics.Abstractions;

public sealed class DefaultCrashEventLoggerSourceInitializer
    : ICrashEventLoggerSourceInitializer
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
        this.IsInitialized = true;
        return Task.CompletedTask;
    }

    #endregion
}
