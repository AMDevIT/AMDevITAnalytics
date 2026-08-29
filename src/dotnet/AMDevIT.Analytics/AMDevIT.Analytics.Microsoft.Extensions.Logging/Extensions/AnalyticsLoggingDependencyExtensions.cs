using AMDevIT.Analytics.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AMDevIT.Analytics.Microsoft.Extensions.Logging.Extensions;

public static class AnalyticsLoggingDependencyExtensions
{
    #region Methods

    public static AnalyticsBuilder AddMicrosoftLogging(this AnalyticsBuilder builder,
                                                       Action<AnalyticsLoggingOptions>? configure = null)
    {
        OptionsBuilder<AnalyticsLoggingOptions> optionsBuilder;

        ArgumentNullException.ThrowIfNull(builder);

        optionsBuilder = builder.Services.AddOptions<AnalyticsLoggingOptions>()
                                .Validate(options => options.QueueCapacity > 0,
                                          "Queue capacity must be greater than zero.")
                                .Validate(options => options.MaximumMessageLength > 0,
                                          "Maximum message length must be greater than zero.")
                                .Validate(options => options.MaximumParameterCount > 0,
                                          "Maximum parameter count must be greater than zero.")
                                .Validate(options => options.FlushTimeout > TimeSpan.Zero,
                                          "Flush timeout must be greater than zero.");

        if (configure != null)
        {
            optionsBuilder.Configure(configure);
        }

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider,
                                                                      AnalyticsLoggerProvider>());

        return builder;
    }

    #endregion
}
