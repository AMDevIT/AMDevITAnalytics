using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AMDevIT.Analytics.Core.Extensions;

/// <summary>Registers the analytics orchestrator and exposes provider configuration through dependency injection.</summary>
public static class AnalyticsDependencyExtensions
{
    #region Methods

    /// <summary>Registers the core analytics services.</summary>
    /// <param name="services">Service collection to extend.</param>
    /// <returns>An analytics builder for additional registrations.</returns>
    public static AnalyticsBuilder AddAMDevITAnalytics(this IServiceCollection services)
    {
        services.TryAddSingleton<IAnalyticsInstance, AnalyticsInstance>();
        return new AnalyticsBuilder(services);
    }

    #endregion
}
