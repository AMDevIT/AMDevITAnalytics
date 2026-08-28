using AMDevIT.Analytics.Abstractions;

namespace AMDevIT.Analytics.Firebase.ManagedDroid
{
    public partial class FirebaseCrashEventLoggerSource
        : ICrashEventLoggerSource
    {
        #region Properties

        public ICrashEventLoggerSourceInitializer Initializer => throw new NotImplementedException();

        public Guid InstanceID
        {
            get;
        }

        #endregion

        #region .ctor

        public FirebaseCrashEventLoggerSource()
        {
            this.InstanceID = Guid.NewGuid();
        }

        #endregion
    }
}
