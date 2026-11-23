namespace InterCase.Common
{
    public sealed class NotFoundException(string message) : Exception(message)
    {
    }
}
