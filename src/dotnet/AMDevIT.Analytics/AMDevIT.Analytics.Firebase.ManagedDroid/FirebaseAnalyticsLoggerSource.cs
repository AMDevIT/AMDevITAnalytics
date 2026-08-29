using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;
using Android.App;
using Android.Content;
using Android.OS;
using Firebase.Analytics;

namespace AMDevIT.Analytics.Firebase.ManagedDroid;

public sealed class FirebaseAnalyticsLoggerSource
    : IAnalyticsLoggerSource, IDisposable
{
    #region Const

    private const string MessageParameter = "message";

    #endregion

    #region Fields

    private readonly Context applicationContext;
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private FirebaseAnalytics? firebaseInstance;
    private bool disposedValue;

    #endregion

    #region Properties

    public Guid InstanceID
    {
        get;
    }

    public bool IsInitialized => this.firebaseInstance != null && !this.disposedValue;

    #endregion

    #region .ctor

    public FirebaseAnalyticsLoggerSource()
        : this(Application.Context)
    {
    }

    public FirebaseAnalyticsLoggerSource(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.applicationContext = context.ApplicationContext ?? context;
        this.InstanceID = Guid.NewGuid();
    }

    #endregion

    #region Methods

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();

        if (this.IsInitialized)
        {
            return;
        }

        await this.initializationLock.WaitAsync(cancellationToken);

        try
        {
            this.ThrowIfDisposed();
            this.firebaseInstance ??= FirebaseAnalytics.GetInstance(this.applicationContext);
        }
        finally
        {
            this.initializationLock.Release();
        }
    }

    public async Task LogEventAsync(AnalyticsEvent analyticsEvent,
                                    CancellationToken cancellationToken = default)
    {
        FirebaseAnalytics firebaseInstance;
        Dictionary<string, object?> parameters;
        Bundle? parametersBundle;

        ArgumentNullException.ThrowIfNull(analyticsEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(analyticsEvent.EventID);

        await this.InitializeAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        firebaseInstance = this.firebaseInstance
            ?? throw new InvalidOperationException("Firebase Analytics is not initialized.");
        parameters = analyticsEvent.Parameters == null
            ? []
            : new Dictionary<string, object?>(analyticsEvent.Parameters);

        if (!string.IsNullOrWhiteSpace(analyticsEvent.Message) &&
            !parameters.ContainsKey(MessageParameter))
        {
            parameters.Add(MessageParameter, analyticsEvent.Message);
        }

        parametersBundle = parameters.ToBundle();
        firebaseInstance.LogEvent(analyticsEvent.EventID, parametersBundle);
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (this.disposedValue)
        {
            return;
        }

        if (disposing)
        {
            this.firebaseInstance?.Dispose();
            this.firebaseInstance = null;
            this.initializationLock.Dispose();
        }

        this.disposedValue = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(this.disposedValue, this);
    }

    #endregion
}
