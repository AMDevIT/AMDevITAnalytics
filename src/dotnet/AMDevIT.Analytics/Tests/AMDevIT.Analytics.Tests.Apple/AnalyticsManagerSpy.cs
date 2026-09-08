using AMDevIT.Analytics.Firebase.BindingApple;
using AMDevIT.Analytics.Firebase.ManagedApple;
using Foundation;

namespace AMDevIT.Analytics.Tests;

internal sealed class AnalyticsManagerSpy : IFirebaseAnalyticsManager
{
    #region Properties

    internal List<string> Calls { get; } = [];
    internal int DisposeCalls { get; private set; }
    internal string? EventName { get; private set; }
    internal Dictionary<string, string>? Parameters { get; private set; }
    internal AnalyticsConsentStatus[] Consent { get; private set; } = [];
    internal double Timeout { get; private set; }
    internal Action<NSNumber, NSError>? SessionCompletion { get; private set; }
    public string? AppInstanceID => "instance";

    #endregion

    #region .ctor

    public AnalyticsManagerSpy()
    {
    }

    #endregion

    #region Methods

    public void Dispose() => this.DisposeCalls++;

    public void LogEventWithName(string name, NSDictionary? parameters)
    {
        this.EventName = name;
        this.Parameters = parameters?.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.ToString());
    }

    public void SetUserID(string? userID) => this.Calls.Add($"user:{userID}");
    public void SetUserProperty(string? value, string name) => this.Calls.Add($"property:{name}:{value}");
    public void SetAnalyticsCollectionEnabled(bool enabled) => this.Calls.Add($"collection:{enabled}");
    public void SetConsentWithAnalyticsStorage(AnalyticsConsentStatus analyticsStorage,
                                               AnalyticsConsentStatus adStorage,
                                               AnalyticsConsentStatus adUserData,
                                               AnalyticsConsentStatus adPersonalization)
    {
        this.Consent = [analyticsStorage, adStorage, adUserData, adPersonalization];
    }

    public void SetDefaultEventParameters(NSDictionary? parameters)
    {
        this.Calls.Add(parameters == null ? "defaults:clear" : $"defaults:{parameters.Count}");
    }

    public void ResetAnalyticsData() => this.Calls.Add("reset");
    public void SetSessionTimeoutInterval(double interval) => this.Timeout = interval;
    public void SessionIDWithCompletion(Action<NSNumber, NSError> completion) => this.SessionCompletion = completion;

    #endregion
}