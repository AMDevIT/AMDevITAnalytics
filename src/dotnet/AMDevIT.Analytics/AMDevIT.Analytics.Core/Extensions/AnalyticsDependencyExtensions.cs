using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMDevIT.Analytics.Core.Extensions
{
    public static class AnalyticsDependencyExtensions
    {
        #region Methods

        public static IServiceCollection UseAMDevITAnalytics(this IServiceCollection services)
        {
            services.AddSingleton<>
        }

        #endregion
    }
}
