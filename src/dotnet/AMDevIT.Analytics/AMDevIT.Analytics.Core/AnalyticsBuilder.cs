using Microsoft.Extensions.DependencyInjection;

namespace AMDevIT.Analytics.Core;

/// <summary>Provides access to the service collection used to register analytics providers.</summary>
public sealed class AnalyticsBuilder
{
    #region Properties

    /// <summary>Gets the service collection being configured.</summary>
    public IServiceCollection Services
    {
        get;
    }

    #endregion

    #region .ctor

    /// <summary>Creates a builder over the specified service collection.</summary>
    /// <param name="services">Service collection being configured.</param>
    internal AnalyticsBuilder(IServiceCollection services)
    {
        this.Services = services;
    }

    #endregion
}
