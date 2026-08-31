using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AMDevIT.Analytics.Firebase.ManagedApple.Extensions;

/// <summary>Registers Apple Firebase sources without initializing the native SDK.</summary>
public static class FirebaseAnalyticsDependencyExtensions
{
    #region Methods

    /// <summary>Registers Firebase Analytics and Crashlytics sources.</summary>
    /// <param name="builder">The analytics builder.</param>
    /// <returns>The same builder.</returns>
    public static AnalyticsBuilder AddFirebase(this AnalyticsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddFirebaseAnalytics().AddFirebaseCrashlytics();
    }

    /// <summary>Registers the Firebase Analytics source.</summary>
    /// <param name="builder">The analytics builder.</param>
    /// <returns>The same builder.</returns>
    public static AnalyticsBuilder AddFirebaseAnalytics(this AnalyticsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAnalyticsLoggerSource, FirebaseAnalyticsLoggerSource>());
        return builder;
    }

    /// <summary>Registers the Firebase Crashlytics source.</summary>
    /// <param name="builder">The analytics builder.</param>
    /// <returns>The same builder.</returns>
    public static AnalyticsBuilder AddFirebaseCrashlytics(this AnalyticsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ICrashEventLoggerSource, FirebaseCrashEventLoggerSource>());
        return builder;
    }

    #endregion
}
