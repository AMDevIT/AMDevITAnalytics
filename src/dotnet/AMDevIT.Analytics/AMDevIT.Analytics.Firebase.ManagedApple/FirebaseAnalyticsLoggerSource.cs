using AMDevIT.Analytics.Abstractions;

namespace AMDevIT.Analytics.Firebase.ManagedApple
{
    public sealed class FirebaseAnalyticsLoggerSource
        : IAnalyticsLoggerSource, IDisposable
    {
        #region Fields

        private bool disposedValue;
        private readonly SemaphoreSlim initializationLock = new(1, 1);

        #endregion

        #region Properties

        public bool Disposed => this.disposedValue;

        public Guid InstanceID => throw new NotImplementedException();

        public bool IsInitialized => throw new NotImplementedException();

        #endregion

        #region Methods

        public Task LogEventAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #region Dispose

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.initializationLock.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion
    }
}
