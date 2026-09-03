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
    void RecordWithError(NSError error, NSDictionary? userInfo);
    void RecordExceptionWithName(string name, string reason, CrashlyticsStackFrame[] stackTrace);
    void SetCustomValue(NSObject? value, string key);
    void SetCustomKeysAndValues(NSDictionary values);
    void SetUserID(string? userID);
    void SetCrashlyticsCollectionEnabled(bool enabled);
    void CheckForUnsentReportsWithCompletion(Action<bool> completion);
    void SendUnsentReports();
    void DeleteUnsentReports();
}