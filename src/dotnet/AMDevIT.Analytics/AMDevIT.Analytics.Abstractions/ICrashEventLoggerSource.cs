namespace AMDevIT.Analytics.Abstractions;

public interface ICrashEventLoggerSource
{
    #region Properties

    ICrashEventLoggerSourceInitializer Initializer
    {
        get;
    }

    Guid InstanceID
    {
        get;
    }

    #endregion
}
