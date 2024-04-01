namespace MpSo.Common.Exceptions;

public class FailedToFetchTagsException : Exception
{
    public FailedToFetchTagsException() : base()
    {
    }

    public FailedToFetchTagsException(string message)
        : base(message)
    {
    }

    public FailedToFetchTagsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
