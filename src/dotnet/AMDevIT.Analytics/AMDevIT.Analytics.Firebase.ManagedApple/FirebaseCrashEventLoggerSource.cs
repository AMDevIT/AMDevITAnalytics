using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Firebase.BindingApple;
using Foundation;
using System.Diagnostics;
using System.Reflection;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Records managed exceptions and exposes Apple Firebase Crashlytics controls.</summary>
/// <remarks>Call <see cref="FirebaseApple.Initialize"/> at application startup before background use.</remarks>
public sealed class FirebaseCrashEventLoggerSource : ICrashEventLoggerSource, IDisposable
{
    #region Fields

    private readonly FirebaseAppleSource<IFirebaseCrashlyticsManager> source;

    #endregion

    #region Properties

    /// <summary>Gets whether this source has been disposed.</summary>
    public bool Disposed => this.source.Disposed;

    /// <inheritdoc />
    public Guid InstanceID { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public bool IsInitialized => this.source.IsInitialized;

    /// <summary>Gets Firebase's current automatic report collection setting.</summary>
    public bool IsCrashlyticsCollectionEnabled => this.source.Read(manager => manager.IsCrashlyticsCollectionEnabled);

    /// <summary>Gets whether Crashlytics detected a crash during the previous execution.</summary>
    public bool DidCrashDuringPreviousExecution => this.source.Read(manager => manager.DidCrashDuringPreviousExecution);

    #endregion

    #region .ctor

    /// <summary>Creates a lazily initialized Apple Crashlytics source.</summary>
    public FirebaseCrashEventLoggerSource() : this(new FirebaseAppleSource<IFirebaseCrashlyticsManager>(() => new FirebaseCrashlyticsManager(), () => FirebaseApple.Initialize()))
    {
    }

    internal FirebaseCrashEventLoggerSource(FirebaseAppleSource<IFirebaseCrashlyticsManager> source)
    {
        this.source = source;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        this.source.Initialize(cancellationToken);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>Event metadata is stored as Crashlytics custom keys and can affect later reports.</remarks>
    public Task LogErrorAsync(CrashEvent crashEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(crashEvent);
        ArgumentNullException.ThrowIfNull(crashEvent.Exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(crashEvent.EventID);

        this.source.Execute(manager =>
        {
            manager.LogWithMessage(crashEvent.EventID);

            if (!string.IsNullOrWhiteSpace(crashEvent.Message))
            {
                manager.LogWithMessage(crashEvent.Message);
            }

            if (crashEvent.Parameters != null)
            {
                using NSDictionary<NSString, NSObject>? parameters = FirebaseAppleParameters.Create(crashEvent.Parameters, customValues: true);
                manager.SetCustomKeysAndValues(parameters!);
            }

            RecordException(manager, crashEvent.Exception);
        }, cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>Adds a diagnostic message without recording an error.</summary>
    /// <param name="message">The diagnostic message.</param>
    public void Log(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        this.source.Execute(manager => manager.LogWithMessage(message));
    }

    /// <summary>Records an NSError and optional event-specific metadata.</summary>
    /// <param name="error">The native error.</param>
    /// <param name="userInfo">Optional event-specific metadata.</param>
    public void RecordError(NSError error, IReadOnlyDictionary<string, object?>? userInfo = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        using NSDictionary<NSString, NSObject>? nativeUserInfo = FirebaseAppleParameters.Create(userInfo, customValues: true);
        this.source.Execute(manager =>
        {
            if (nativeUserInfo == null)
            {
                manager.RecordWithError(error);
            }
            else
            {
                manager.RecordWithError(error, nativeUserInfo);
            }
        });
    }

    /// <summary>Records a managed exception with its available managed stack frames.</summary>
    /// <param name="exception">The managed exception.</param>
    public void RecordException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        this.source.Execute(manager => RecordException(manager, exception));
    }

    /// <summary>Sets a custom value used by subsequent reports. Unsupported values are formatted invariantly.</summary>
    /// <param name="key">The custom key.</param>
    /// <param name="value">The custom value.</param>
    public void SetCustomValue(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        using NSObject nativeValue = FirebaseAppleParameters.CreateValue(value, customValues: true);
        this.source.Execute(manager => manager.SetCustomValue(nativeValue, key));
    }

    /// <summary>Merges custom values used by subsequent reports.</summary>
    /// <param name="values">The custom keys and values.</param>
    public void SetCustomKeysAndValues(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        using NSDictionary<NSString, NSObject>? nativeValues = FirebaseAppleParameters.Create(values, customValues: true);
        this.source.Execute(manager => manager.SetCustomKeysAndValues(nativeValues!));
    }

    /// <summary>Sets or clears the Crashlytics user identifier.</summary>
    /// <param name="userID">The identifier, or null to clear it.</param>
    public void SetUserID(string? userID) => this.source.Execute(manager => manager.SetUserID(userID));

    /// <summary>Sets Firebase's persisted automatic report collection override.</summary>
    /// <param name="enabled">Whether automatic report collection is enabled.</param>
    public void SetCrashlyticsCollectionEnabled(bool enabled) => this.source.Execute(manager => manager.SetCrashlyticsCollectionEnabled(enabled));

    /// <summary>Checks for pending reports. Call once per launch when automatic collection is disabled.</summary>
    /// <param name="cancellationToken">Token used to cancel the managed wait.</param>
    /// <returns>True when pending reports are available.</returns>
    /// <remarks>Cancellation only cancels the managed wait.</remarks>
    public Task<bool> CheckForUnsentReportsAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        this.source.Execute(manager => manager.CheckForUnsentReportsWithCompletion(available => completion.TrySetResult(available)),
                            cancellationToken);
        return FirebaseAppleCallback.WaitAsync(completion, cancellationToken);
    }

    /// <summary>Requests upload of pending reports when automatic collection is disabled.</summary>
    public void SendUnsentReports() => this.source.Execute(manager => manager.SendUnsentReports());

    /// <summary>Deletes pending local reports when automatic collection is disabled.</summary>
    public void DeleteUnsentReports() => this.source.Execute(manager => manager.DeleteUnsentReports());

    /// <summary>Releases this native manager without shutting down the shared Firebase app.</summary>
    public void Dispose() => this.source.Dispose();

    private static void RecordException(IFirebaseCrashlyticsManager manager, Exception exception)
    {
        List<CrashlyticsStackFrame> nativeFrames = [];
        string name = exception.GetType().FullName ?? exception.GetType().Name;
        string reason = string.IsNullOrWhiteSpace(exception.Message) ? name : exception.Message;

        try
        {
            foreach (StackFrame frame in new StackTrace(exception, true).GetFrames() ?? [])
            {
                MethodBase? method = frame.GetMethod();
                string symbol = method == null
                    ? frame.ToString()?.Trim() ?? "unknown"
                    : $"{method.DeclaringType?.FullName ?? "unknown"}.{method.Name}";
                string file = frame.GetFileName() ?? string.Empty;
                int line = Math.Max(frame.GetFileLineNumber(), 0);
                nativeFrames.Add(new CrashlyticsStackFrame(symbol, file, line));
            }

            manager.RecordExceptionWithName(name, reason, nativeFrames.ToArray());
        }
        finally
        {
            foreach (CrashlyticsStackFrame frame in nativeFrames)
            {
                frame.Dispose();
            }
        }
    }

    #endregion
}
