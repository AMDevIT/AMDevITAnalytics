namespace AMDevIT.Analytics.Core
{
    public class AnalyticsInstance(ICrashEventLogger crashEventLogger,
                                   IAnalyticsEventLogger analyticsEventLogger)
        : IAnalyticsInstance
    {
        #region Properties

        public ICrashEventLogger CrashEventLogger => crashEventLogger;

        public IAnalyticsEventLogger AnalyticsEventLogger => analyticsEventLogger;

        #endregion
    }
}
