namespace AMDevIT.Analytics.Abstractions;

public interface ICrashEventLoggerSourceInitializer
{
    #region Properties

    bool IsInitialized
    {
        get;
    }

    #endregion

    #region Methods

    Task InitializeAsync(CancellationToken cancellationToken = default);

    #endregion
}
