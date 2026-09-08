using AMDevIT.Analytics.Firebase.BindingApple;
using Foundation;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Managed boundary for the native Swift manager, which cannot be subclassed.</summary>
internal interface IFirebaseAnalyticsManager : IDisposable
{
    string? AppInstanceID { get; }

    void LogEventWithName(string name, NSDictionary? parameters);
    void SetUserID(string? userID);
    void SetUserProperty(string? value, string name);
    void SetAnalyticsCollectionEnabled(bool enabled);
    void SetConsentWithAnalyticsStorage(AnalyticsConsentStatus analyticsStorage, AnalyticsConsentStatus adStorage, AnalyticsConsentStatus adUserData, AnalyticsConsentStatus adPersonalization);
    void SetDefaultEventParameters(NSDictionary? parameters);
    void ResetAnalyticsData();
    void SetSessionTimeoutInterval(double interval);
    void SessionIDWithCompletion(Action<NSNumber, NSError> completion);
}