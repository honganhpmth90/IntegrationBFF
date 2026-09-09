using Confluent.Kafka;
using IntegrationBFF.Application.Abstractions.Messaging;
using IntegrationBFF.Application.Abstractions.Partners;
using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Application.Options;
using IntegrationBFF.Infrastructure.Messaging;
using IntegrationBFF.Infrastructure.Partners;
using IntegrationBFF.Infrastructure.Persistence;
using IntegrationBFF.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IntegrationBFF.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IntegrationBff")
            ?? throw new InvalidOperationException("Connection string 'IntegrationBff' is missing.");

        services.AddDbContext<IntegrationBffDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IntegrationBffDbContext>());

        services.AddOptions<PartnerVerificationOptions>()
            .Bind(configuration.GetSection(PartnerVerificationOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "BaseUrl must be absolute.")
            .Validate(options => options.MaxAttempts > 0, "MaxAttempts must be greater than zero.")
            .ValidateOnStart();

        services.AddHttpClient<IPartnerVerificationClient, PartnerVerificationHttpClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<PartnerVerificationOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromMilliseconds(Math.Max(1, options.TimeoutMilliseconds));
        });

        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers), "BootstrapServers is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.TransactionTopic), "TransactionTopic is required.")
            .ValidateOnStart();

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;
            return new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 5
            }).Build();
        });
        services.AddScoped<ITransactionPublisher, KafkaTransactionPublisher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
