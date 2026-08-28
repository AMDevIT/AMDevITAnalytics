namespace AMDevIT.Analytics.Abstractions;

public interface IAnalyticsLoggerSourceInitializerBuilder
{
    #region Methods

    public Task<IAnalyticsLoggerSourceInitializer> BuildAsync(CancellationToken cancellationToken = default);

    #endregion
}
