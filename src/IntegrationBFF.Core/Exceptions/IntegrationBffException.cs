namespace IntegrationBFF.Core.Exceptions;

public abstract class IntegrationBffException : Exception
{
    protected IntegrationBffException(string message) : base(message)
    {
    }

    protected IntegrationBffException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
