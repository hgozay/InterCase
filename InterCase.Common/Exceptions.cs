namespace InterCase.Common
{
    public sealed class NotFoundException(string message) : Exception(message)
    {
    }

    public sealed class CacheUnavailableException(string message, Exception innerException) : Exception(message, innerException)
    {
    }
}
