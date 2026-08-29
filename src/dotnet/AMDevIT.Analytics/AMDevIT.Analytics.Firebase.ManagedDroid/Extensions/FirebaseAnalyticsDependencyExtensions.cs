using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;

public static class FirebaseAnalyticsDependencyExtensions
{
    #region Methods

    /// <summary>Registers Firebase Analytics and Crashlytics sources.</summary>
    /// <param name="builder">Analytics builder to extend.</param>
    /// <returns>The same builder.</returns>
    public static AnalyticsBuilder AddFirebase(this AnalyticsBuilder builder)
    {
        builder.AddFirebaseAnalytics();
        builder.AddFirebaseCrashlytics();
        return builder;
    }

    /// <summary>Registers the Firebase Analytics source.</summary>
    /// <param name="builder">Analytics builder to extend.</param>
    /// <returns>The same builder.</returns>
    public static AnalyticsBuilder AddFirebaseAnalytics(this AnalyticsBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAnalyticsLoggerSource,
                                          FirebaseAnalyticsLoggerSource>());

        return builder;
    }

    /// <summary>Registers the Firebase Crashlytics source.</summary>
    /// <param name="builder">Analytics builder to extend.</param>
    /// <returns>The same builder.</returns>
    public static AnalyticsBuilder AddFirebaseCrashlytics(this AnalyticsBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ICrashEventLoggerSource,
                                          FirebaseCrashEventLoggerSource>());

        return builder;
    }

    #endregion
}
