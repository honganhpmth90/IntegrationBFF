using Confluent.Kafka;
using IntegrationBFF.Application.Abstractions.Messaging;
using Microsoft.Extensions.Options;

namespace IntegrationBFF.Infrastructure.Messaging;

internal sealed class KafkaTransactionPublisher(
    IProducer<string, string> producer,
    IOptions<KafkaOptions> options) : ITransactionPublisher
{
    private readonly KafkaOptions _options = options.Value;

    public Task PublishAsync(string key, string payload, CancellationToken cancellationToken) =>
        producer.ProduceAsync(
            _options.TransactionTopic,
            new Message<string, string> { Key = key, Value = payload },
            cancellationToken);
}
