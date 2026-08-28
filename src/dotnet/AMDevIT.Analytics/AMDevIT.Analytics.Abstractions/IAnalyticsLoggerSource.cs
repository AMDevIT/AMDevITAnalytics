namespace AMDevIT.Analytics.Abstractions;

public interface IAnalyticsLoggerSource
{
    #region Properties

    IAnalyticsLoggerSourceInitializer Initializer
    {
        get;
    }

    Guid InstanceID
    {
        get;
    }

    #endregion
}
