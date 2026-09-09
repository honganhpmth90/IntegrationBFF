namespace IntegrationBFF.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = "localhost:9092";
    public string TransactionTopic { get; init; } = "partner-transactions";
}
