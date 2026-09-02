using AMDevIT.Analytics.Firebase.BindingApple;
using Foundation;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Managed boundary for the native Swift manager, which cannot be subclassed.</summary>
internal interface IFirebaseCrashlyticsManager : IDisposable
{
    bool IsCrashlyticsCollectionEnabled { get; }
    bool DidCrashDuringPreviousExecution { get; }

    void LogWithMessage(string message);
    void RecordWithError(NSError error);
    void RecordWithError(NSError error, NSDictionary<NSString, NSObject>? userInfo);
    void RecordExceptionWithName(string name, string reason, CrashlyticsStackFrame[] stackTrace);
    void SetCustomValue(NSObject? value, string key);
    void SetCustomKeysAndValues(NSDictionary<NSString, NSObject> values);
    void SetUserID(string? userID);
    void SetCrashlyticsCollectionEnabled(bool enabled);
    void CheckForUnsentReportsWithCompletion(Action<bool> completion);
    void SendUnsentReports();
    void DeleteUnsentReports();
}

/// <summary>Forwards calls to the binding; lifetime is owned by FirebaseAppleSource.</summary>
internal sealed class FirebaseCrashlyticsManager : IFirebaseCrashlyticsManager
{
    #region Fields

    private readonly CrashlyticsManager manager = new();

    #endregion

    #region Properties

    public bool IsCrashlyticsCollectionEnabled => this.manager.IsCrashlyticsCollectionEnabled;
    public bool DidCrashDuringPreviousExecution => this.manager.DidCrashDuringPreviousExecution;

    #endregion

    #region Methods

    public void LogWithMessage(string message) => this.manager.LogWithMessage(message);
    public void RecordWithError(NSError error) => this.manager.RecordWithError(error);
    public void RecordWithError(NSError error, NSDictionary<NSString, NSObject>? userInfo) => this.manager.RecordWithError(error, userInfo);
    public void RecordExceptionWithName(string name, string reason, CrashlyticsStackFrame[] stackTrace) => this.manager.RecordExceptionWithName(name, reason, stackTrace);
    public void SetCustomValue(NSObject? value, string key) => this.manager.SetCustomValue(value, key);
    public void SetCustomKeysAndValues(NSDictionary<NSString, NSObject> values) => this.manager.SetCustomKeysAndValues(values);
    public void SetUserID(string? userID) => this.manager.SetUserID(userID);
    public void SetCrashlyticsCollectionEnabled(bool enabled) => this.manager.SetCrashlyticsCollectionEnabled(enabled);
    public void CheckForUnsentReportsWithCompletion(Action<bool> completion) => this.manager.CheckForUnsentReportsWithCompletion(completion);
    public void SendUnsentReports() => this.manager.SendUnsentReports();
    public void DeleteUnsentReports() => this.manager.DeleteUnsentReports();
    public void Dispose() => this.manager.Dispose();

    #endregion
}
