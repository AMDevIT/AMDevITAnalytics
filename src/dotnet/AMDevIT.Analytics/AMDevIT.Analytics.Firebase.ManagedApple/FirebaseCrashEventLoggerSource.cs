using AMDevIT.Analytics.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

public sealed class FirebaseCrashEventLoggerSource
    : ICrashEventLoggerSource, IDisposable
{
    #region Fields

    private bool disposedValue;
    private readonly SemaphoreSlim initializationLock = new(1, 1);

    #endregion

    #region Properties

    public Guid InstanceID => throw new NotImplementedException();

    public bool IsInitialized => throw new NotImplementedException();

    #endregion

    #region Methods

    public Task LogErrorAsync(CrashEvent crashEvent, CancellationToken cancellationToken = default)
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
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }  

    #endregion

    #endregion
}
