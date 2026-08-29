namespace AMDevIT.Analytics.Core;

public sealed class AnalyticsSourceOperationException
    : Exception
{
    #region Properties

    public Guid SourceInstanceID
    {
        get;
    }

    public Type SourceType
    {
        get;
    }

    public string Operation
    {
        get;
    }

    #endregion

    #region .ctor

    internal AnalyticsSourceOperationException(Guid sourceInstanceID,
                                                Type sourceType,
                                                string operation,
                                                Exception innerException)
        : base($"Analytics source '{sourceType.FullName}' failed during '{operation}'.",
               innerException)
    {
        this.SourceInstanceID = sourceInstanceID;
        this.SourceType = sourceType;
        this.Operation = operation;
    }

    #endregion
}
