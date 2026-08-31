using AMDevIT.Analytics.Abstractions;

namespace AMDevIT.Analytics.Firebase.ManagedApple
{
    /// <summary>Provides the managed Apple Firebase analytics source contract.</summary>
    /// <remarks>Initialization, identity access, and event reporting are not implemented yet.</remarks>
    public sealed class FirebaseAnalyticsLoggerSource
        : IAnalyticsLoggerSource, IDisposable
    {
        #region Fields

        private bool disposedValue;
        private readonly SemaphoreSlim initializationLock = new(1, 1);

        #endregion

        #region Properties

        /// <summary>Gets whether this source has been disposed.</summary>
        public bool Disposed => this.disposedValue;

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">This property is not implemented yet.</exception>
        public Guid InstanceID => throw new NotImplementedException();

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">This property is not implemented yet.</exception>
        public bool IsInitialized => throw new NotImplementedException();

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Event reporting is not implemented yet.</exception>
        public Task LogEventAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Initialization is not implemented yet.</exception>
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

        /// <summary>Releases the managed resources owned by this source.</summary>
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
