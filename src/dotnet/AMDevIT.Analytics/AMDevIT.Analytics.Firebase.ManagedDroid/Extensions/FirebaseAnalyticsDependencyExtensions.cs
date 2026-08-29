using AMDevIT.Analytics.Abstractions;
using AMDevIT.Analytics.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;

public static class FirebaseAnalyticsDependencyExtensions
{
    #region Methods

    public static AnalyticsBuilder AddFirebase(this AnalyticsBuilder builder)
    {
        builder.AddFirebaseAnalytics();
        builder.AddFirebaseCrashlytics();
        return builder;
    }

    public static AnalyticsBuilder AddFirebaseAnalytics(this AnalyticsBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IAnalyticsLoggerSource,
                                        FirebaseAnalyticsLoggerSource>());

        return builder;
    }

    public static AnalyticsBuilder AddFirebaseCrashlytics(this AnalyticsBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ICrashEventLoggerSource,
                                        FirebaseCrashEventLoggerSource>());

        return builder;
    }

    #endregion
}
