using AMDevIT.Analytics.Abstractions;
using Android.Content;
using Firebase.Analytics;

namespace AMDevIT.Analytics.Firebase.ManagedDroid;

public partial class FirebaseAnalyticsLoggerSourceInitializer
    : IAnalyticsLoggerSourceInitializer, IDisposable
{
    #region Fields

    private readonly Context context;
    private FirebaseAnalytics? firebaseInstace;
    private bool disposedValue;

    #endregion

    #region Properties

    internal Context Context => this.context;

    internal FirebaseAnalytics? FirebaseInstance => this.firebaseInstace;

    public bool IsInitialized => throw new NotImplementedException();

    public bool Disposed => this.disposedValue;       

    #endregion

    #region .ctor

    internal FirebaseAnalyticsLoggerSourceInitializer(Context context)
    {
        this.context = context;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            FirebaseAnalytics currentInstance = FirebaseAnalytics.GetInstance(this.context);
            this.firebaseInstace = currentInstance;
        }
        catch(Exception exc)
        {
            return Task.FromException(exc);
        }

        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.firebaseInstace?.Dispose();
                this.firebaseInstace = null;
            }
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
