namespace AMDevIT.Analytics.Abstractions;

/// <summary>Defines the identity and initialization lifecycle shared by analytics and crash sources.</summary>
public interface IAnalyticsSource
{
    #region Properties

    /// <summary>Gets the identifier of this source instance.</summary>
    Guid InstanceID
    {
        get;
    }

    /// <summary>Gets whether this source has completed initialization.</summary>
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
