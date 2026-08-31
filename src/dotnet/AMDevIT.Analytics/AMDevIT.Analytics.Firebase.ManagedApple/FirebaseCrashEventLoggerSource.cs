using AMDevIT.Analytics.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMDevIT.Analytics.Firebase.ManagedApple;

/// <summary>Provides the managed Apple Firebase crash-reporting source contract.</summary>
/// <remarks>Initialization, identity access, and crash reporting are not implemented yet.</remarks>
public sealed class FirebaseCrashEventLoggerSource
    : ICrashEventLoggerSource, IDisposable
{
    #region Fields

    private bool disposedValue;
    private readonly SemaphoreSlim initializationLock = new(1, 1);

    #endregion

    #region Properties

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">This property is not implemented yet.</exception>
    public Guid InstanceID => throw new NotImplementedException();

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">This property is not implemented yet.</exception>
    public bool IsInitialized => throw new NotImplementedException();

    #endregion

    #region Methods

    /// <inheritdoc />
    /// <exception cref="NotImplementedException">Crash reporting is not implemented yet.</exception>
    public Task LogErrorAsync(CrashEvent crashEvent, CancellationToken cancellationToken = default)
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
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }  

    #endregion

    #endregion
}
