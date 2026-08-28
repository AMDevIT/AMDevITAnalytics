namespace AMDevIT.Analytics.Core;

public interface IAnalyticsInstance
{
    #region Properties

    ICrashEventLogger CrashEventLogger
    {
        get;
    }

    IAnalyticsEventLogger AnalyticsEventLogger
    {
        get;
    }

    #endregion
}
