using AMDevIT.Analytics.Firebase.BindingApple;
using Foundation;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Forwards calls to the binding; lifetime is owned by FirebaseAppleSource.</summary>
internal sealed class FirebaseAnalyticsManager : IFirebaseAnalyticsManager
{
    #region Fields

    private readonly AnalyticsManager manager = new();

    #endregion

    #region Properties

    public string? AppInstanceID => this.manager.AppInstanceID;

    #endregion

    #region Methods

    public void LogEventWithName(string name, NSDictionary? parameters) => this.manager.LogEventWithName(name, parameters);
    public void SetUserID(string? userID) => this.manager.SetUserID(userID);
    public void SetUserProperty(string? value, string name) => this.manager.SetUserProperty(value, name);
    public void SetAnalyticsCollectionEnabled(bool enabled) => this.manager.SetAnalyticsCollectionEnabled(enabled);
    public void SetConsentWithAnalyticsStorage(AnalyticsConsentStatus analyticsStorage, AnalyticsConsentStatus adStorage, AnalyticsConsentStatus adUserData, AnalyticsConsentStatus adPersonalization) => this.manager.SetConsentWithAnalyticsStorage(analyticsStorage, adStorage, adUserData, adPersonalization);
    public void SetDefaultEventParameters(NSDictionary? parameters) => this.manager.SetDefaultEventParameters(parameters);
    public void ResetAnalyticsData() => this.manager.ResetAnalyticsData();
    public void SetSessionTimeoutInterval(double interval) => this.manager.SetSessionTimeoutInterval(interval);
    public void SessionIDWithCompletion(Action<NSNumber, NSError> completion) => this.manager.SessionIDWithCompletion(completion);
    public void Dispose() => this.manager.Dispose();

    #endregion
}
