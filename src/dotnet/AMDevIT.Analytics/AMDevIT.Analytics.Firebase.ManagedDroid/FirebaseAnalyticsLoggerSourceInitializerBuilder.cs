using AMDevIT.Analytics.Abstractions;
using Android.Content;

namespace AMDevIT.Analytics.Firebase.ManagedDroid;

public class FirebaseAnalyticsLoggerSourceInitializerBuilder
    : IAnalyticsLoggerSourceInitializerBuilder
{
    #region Fields

    private Context? context = null;

    #endregion

    #region Properties

    public Context? Context => this.context;    

    #endregion

    #region Methods

    public FirebaseAnalyticsLoggerSourceInitializerBuilder UsingContext(Context context)
    {
        this.context = context;
        return this;
    }

    public Task<IAnalyticsLoggerSourceInitializer> BuildAsync(CancellationToken cancellationToken = default)
    {
        if (this.context == null)
        {
            throw new InvalidOperationException("Cannot build initializer without context");
        }

        FirebaseAnalyticsLoggerSourceInitializer firebaseAnalyticsLoggerSourceInitializer = new(this.context);
        return Task.FromResult<IAnalyticsLoggerSourceInitializer>(firebaseAnalyticsLoggerSourceInitializer);
    }

    #endregion
}
