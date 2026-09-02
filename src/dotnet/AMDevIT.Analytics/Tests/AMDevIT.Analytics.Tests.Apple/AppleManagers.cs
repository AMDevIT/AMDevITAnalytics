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

    public void LogEventWithName(string name, NSDictionary<NSString, NSObject>? parameters)
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

    public void SetDefaultEventParameters(NSDictionary<NSString, NSObject>? parameters)
    {
        this.Calls.Add(parameters == null ? "defaults:clear" : $"defaults:{parameters.Count}");
    }

    public void ResetAnalyticsData() => this.Calls.Add("reset");
    public void SetSessionTimeoutInterval(double interval) => this.Timeout = interval;
    public void SessionIDWithCompletion(Action<NSNumber, NSError> completion) => this.SessionCompletion = completion;

    #endregion
}

internal sealed class CrashlyticsManagerSpy : IFirebaseCrashlyticsManager
{
    #region Properties

    internal List<string> Calls { get; } = [];
    internal string? ExceptionName { get; private set; }
    internal string? Reason { get; private set; }
    internal string[] Symbols { get; private set; } = [];
    internal NSError? Error { get; private set; }
    internal Action<bool>? ReportsCompletion { get; private set; }
    public bool IsCrashlyticsCollectionEnabled => false;
    public bool DidCrashDuringPreviousExecution => true;

    #endregion

    #region .ctor

    public CrashlyticsManagerSpy()
    {
    }

    #endregion

    #region Methods

    public void Dispose() { }

    public void LogWithMessage(string message) => this.Calls.Add($"log:{message}");
    public void RecordWithError(NSError error)
    {
        this.Error = error;
        this.Calls.Add("error");
    }

    public void RecordWithError(NSError error, NSDictionary<NSString, NSObject>? userInfo)
    {
        this.Error = error;
        this.Calls.Add($"error:{userInfo?.Count}");
    }

    public void RecordExceptionWithName(string name, string reason, CrashlyticsStackFrame[] stackTrace)
    {
        this.ExceptionName = name;
        this.Reason = reason;
        this.Symbols = stackTrace.Select(frame => frame.Symbol).ToArray();
        this.Calls.Add("exception");
    }

    public void SetCustomValue(NSObject? value, string key) => this.Calls.Add($"value:{key}:{value}");
    public void SetCustomKeysAndValues(NSDictionary<NSString, NSObject> values) => this.Calls.Add($"keys:{values.Count}");
    public void SetUserID(string? userID) => this.Calls.Add($"user:{userID}");
    public void SetCrashlyticsCollectionEnabled(bool enabled) => this.Calls.Add($"collection:{enabled}");
    public void CheckForUnsentReportsWithCompletion(Action<bool> completion) => this.ReportsCompletion = completion;
    public void SendUnsentReports() => this.Calls.Add("send");
    public void DeleteUnsentReports() => this.Calls.Add("delete");

    #endregion
}
