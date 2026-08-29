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

    /// <summary>Creates an exception describing a failed source operation.</summary>
    /// <param name="sourceInstanceID">Failed source instance identifier.</param>
    /// <param name="sourceType">Failed source type.</param>
    /// <param name="operation">Operation that failed.</param>
    /// <param name="innerException">Original provider exception.</param>
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
