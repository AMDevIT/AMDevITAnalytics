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

    /// <summary>Initializes the analytics source.</summary>
    /// <param name="cancellationToken">Token used to cancel initialization.</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    #endregion
}
