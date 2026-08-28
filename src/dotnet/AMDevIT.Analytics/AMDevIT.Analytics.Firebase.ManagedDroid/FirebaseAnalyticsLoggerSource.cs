using AMDevIT.Analytics.Abstractions;
using Firebase.Analytics;
using AMDevIT.Analytics.Firebase.ManagedDroid.Extensions;

namespace AMDevIT.Analytics.Firebase.ManagedDroid
{
    public partial class FirebaseAnalyticsLoggerSource
        : IAnalyticsLoggerSource
    {
        #region Properties

        public IAnalyticsLoggerSourceInitializer Initializer => throw new NotImplementedException();

        public Guid InstanceID => throw new NotImplementedException();

        #endregion

        #region Methods

        public async Task LogEvent(string eventID, 
                                   string message, 
                                   IReadOnlyDictionary<string, object> parameters, 
                                   CancellationToken cancellationToken = default)
        {
            FirebaseAnalytics? instance;
            Bundle? parametersBundle = null;

            if (this.Initializer.IsInitialized == true &&
                this.Initializer is FirebaseAnalyticsLoggerSourceInitializer firebaseAnalyticsLoggerInitializer)
            {
                instance = firebaseAnalyticsLoggerInitializer.FirebaseInstance;
            }
            else
                throw new InvalidOperationException("Firebase Analytics not initialized.");

            if (parameters != null)
            {
                parametersBundle = parameters.ToBundle();
            }

            instance?.LogEvent(eventID, parametersBundle);
        }

        #endregion
    }
}
