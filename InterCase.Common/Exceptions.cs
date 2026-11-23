namespace InterCase.Common
{
    public sealed class NotFoundException(string message) : Exception(message)
    {
    }

    public sealed class ConflictException(string messageKey) : Exception(messageKey)
    {
        public string MessageKey { get; } = messageKey;
    }

    public sealed class CacheUnavailableException(string message, Exception innerException) : Exception(message, innerException)
    {
    }
}
