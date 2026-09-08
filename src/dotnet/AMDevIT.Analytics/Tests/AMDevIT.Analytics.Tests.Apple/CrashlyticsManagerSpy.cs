using AMDevIT.Analytics.Firebase.BindingApple;
using AMDevIT.Analytics.Firebase.ManagedApple;
using Foundation;

namespace AMDevIT.Analytics.Tests;

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

    public void RecordWithError(NSError error, NSDictionary? userInfo)
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
    public void SetCustomKeysAndValues(NSDictionary? values) => this.Calls.Add($"keys:{values.Count}");
    public void SetUserID(string? userID) => this.Calls.Add($"user:{userID}");
    public void SetCrashlyticsCollectionEnabled(bool enabled) => this.Calls.Add($"collection:{enabled}");
    public void CheckForUnsentReportsWithCompletion(Action<bool> completion) => this.ReportsCompletion = completion;
    public void SendUnsentReports() => this.Calls.Add("send");
    public void DeleteUnsentReports() => this.Calls.Add("delete");

    #endregion
}
