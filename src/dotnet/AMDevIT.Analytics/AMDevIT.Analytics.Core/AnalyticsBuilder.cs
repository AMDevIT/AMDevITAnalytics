using Microsoft.Extensions.DependencyInjection;

namespace AMDevIT.Analytics.Core;

public sealed class AnalyticsBuilder
{
    #region Properties

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
