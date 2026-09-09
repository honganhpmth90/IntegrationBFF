namespace IntegrationBFF.Application.Abstractions.Messaging;

public interface ITransactionPublisher
{
    Task PublishAsync(string key, string payload, CancellationToken cancellationToken);
}
