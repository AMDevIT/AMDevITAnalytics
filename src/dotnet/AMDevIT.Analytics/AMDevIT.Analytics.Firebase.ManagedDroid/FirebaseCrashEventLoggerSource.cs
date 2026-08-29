using AMDevIT.Analytics.Abstractions;
using Firebase.Crashlytics;

namespace AMDevIT.Analytics.Firebase.ManagedDroid;

public sealed class FirebaseCrashEventLoggerSource
    : ICrashEventLoggerSource
{
    #region Fields

    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private FirebaseCrashlytics? firebaseInstance;

    #endregion

    #region Properties

    public Guid InstanceID
    {
        get;
    }

    public bool IsInitialized => this.firebaseInstance != null;

    #endregion

    #region .ctor

    public FirebaseCrashEventLoggerSource()
    {
        this.InstanceID = Guid.NewGuid();
    }

    #endregion

    #region Methods

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.IsInitialized)
        {
            return;
        }

        await this.initializationLock.WaitAsync(cancellationToken);

        try
        {
            this.firebaseInstance ??= FirebaseCrashlytics.Instance;
        }
        finally
        {
            this.initializationLock.Release();
        }
    }

    public async Task LogErrorAsync(CrashEvent crashEvent,
                                    CancellationToken cancellationToken = default)
    {
        FirebaseCrashlytics firebaseInstance;
        Java.Lang.Throwable throwable;

        ArgumentNullException.ThrowIfNull(crashEvent);
        ArgumentNullException.ThrowIfNull(crashEvent.Exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(crashEvent.EventID);

        await this.InitializeAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        firebaseInstance = this.firebaseInstance
            ?? throw new InvalidOperationException("Firebase Crashlytics is not initialized.");

        firebaseInstance.Log(crashEvent.EventID);

        if (!string.IsNullOrWhiteSpace(crashEvent.Message))
        {
            firebaseInstance.Log(crashEvent.Message);
        }

        if (crashEvent.Parameters != null)
        {
            foreach (KeyValuePair<string, object?> parameter in crashEvent.Parameters)
            {
                SetCustomKey(firebaseInstance,
                             parameter.Key,
                             parameter.Value);
            }
        }

        throwable = Java.Lang.Throwable.FromException(crashEvent.Exception);
        firebaseInstance.RecordException(throwable);
    }

    private static void SetCustomKey(FirebaseCrashlytics firebaseInstance,
                                     string key,
                                     object? value)
    {
        switch (value)
        {
            case null:
                firebaseInstance.SetCustomKey(key, string.Empty);
                break;
            case bool boolValue:
                firebaseInstance.SetCustomKey(key, boolValue);
                break;
            case byte byteValue:
                firebaseInstance.SetCustomKey(key, (long)byteValue);
                break;
            case sbyte sbyteValue:
                firebaseInstance.SetCustomKey(key, (long)sbyteValue);
                break;
            case short shortValue:
                firebaseInstance.SetCustomKey(key, (long)shortValue);
                break;
            case ushort ushortValue:
                firebaseInstance.SetCustomKey(key, (long)ushortValue);
                break;
            case int intValue:
                firebaseInstance.SetCustomKey(key, (long)intValue);
                break;
            case uint uintValue:
                firebaseInstance.SetCustomKey(key, (long)uintValue);
                break;
            case long longValue:
                firebaseInstance.SetCustomKey(key, longValue);
                break;
            case ulong ulongValue when ulongValue <= long.MaxValue:
                firebaseInstance.SetCustomKey(key, (long)ulongValue);
                break;
            case float floatValue:
                firebaseInstance.SetCustomKey(key, floatValue);
                break;
            case double doubleValue:
                firebaseInstance.SetCustomKey(key, doubleValue);
                break;
            case decimal decimalValue:
                firebaseInstance.SetCustomKey(key, (double)decimalValue);
                break;
            default:
                firebaseInstance.SetCustomKey(key, value.ToString() ?? string.Empty);
                break;
        }
    }

    #endregion
}
