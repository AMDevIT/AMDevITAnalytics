using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AMDevIT.Analytics.Core.Extensions;

public static class AnalyticsDependencyExtensions
{
    #region Methods

    public static AnalyticsBuilder AddAMDevITAnalytics(this IServiceCollection services)
    {
        services.TryAddSingleton<IAnalyticsInstance, AnalyticsInstance>();
        return new AnalyticsBuilder(services);
    }

    #endregion
}
