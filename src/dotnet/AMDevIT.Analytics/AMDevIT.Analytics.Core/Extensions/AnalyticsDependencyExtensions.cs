using Microsoft.Extensions.DependencyInjection;

namespace AMDevIT.Analytics.Core.Extensions;

public static class AnalyticsDependencyExtensions
{
    #region Methods

    public static IServiceCollection UseAMDevITAnalytics(this IServiceCollection services)
    {
        services.AddSingleton<IAnalyticsInstance, AnalyticsInstance>();
        return services;
    }

    #endregion
}
