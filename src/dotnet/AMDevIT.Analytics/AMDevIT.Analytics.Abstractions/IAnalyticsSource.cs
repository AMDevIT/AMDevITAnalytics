namespace AMDevIT.Analytics.Abstractions;

public interface IAnalyticsSource
{
    #region Properties

    Guid InstanceID
    {
        get;
    }

    bool IsInitialized
    {
        get;
    }

    #endregion

    #region Methods

    Task InitializeAsync(CancellationToken cancellationToken = default);

    #endregion
}
