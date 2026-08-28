using AMDevIT.Analytics.Abstractions;
using System.Collections.Concurrent;

namespace AMDevIT.Analytics.Core
{
    public class AnalyticsInstance(ICrashEventLogger crashEventLogger,
                                   IAnalyticsEventLogger analyticsEventLogger)
        : IAnalyticsInstance
    {
        #region Fields

        private readonly ConcurrentDictionary<string, IAnalyticsLoggerSource> analyticsLoggers = [];

        #endregion

        #region Properties

        #endregion

        #region Methods

        public async Task LogEventAsync(string eventID, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (IAnalyticsLoggerSource loggerSource in analyticsLoggers.Values)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (loggerSource.Initializer.IsInitialized)
                        {

                        }
                    }
                }, cancellationToken);
            }
            catch(TaskCanceledException)
            {
                throw;
            }
            catch(Exception exc)
            {

            }
        }

        public Task LogErrorAsync(Exception exception, string eventID, string message, CancellationToken cancellationToken = default)
        {

            throw new NotImplementedException();
        }

        #endregion
    }
}
