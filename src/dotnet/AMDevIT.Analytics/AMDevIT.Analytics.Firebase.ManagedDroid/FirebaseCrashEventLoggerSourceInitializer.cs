using AMDevIT.Analytics.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMDevIT.Analytics.Firebase.ManagedDroid
{
    public class FirebaseCrashEventLoggerSourceInitializer
        : ICrashEventLoggerSourceInitializer
    {
        #region Properties

        public bool IsInitialized => throw new NotImplementedException();

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
