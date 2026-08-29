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

    internal AnalyticsBuilder(IServiceCollection services)
    {
        this.Services = services;
    }

    #endregion
}
