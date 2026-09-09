using System.Text.Json;
using IntegrationBFF.Application.Abstractions.Messaging;
using IntegrationBFF.Application.Transactions.Contracts;
using IntegrationBFF.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationBFF.Infrastructure.Messaging;

internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unexpected error while processing the transactional outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationBffDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<ITransactionPublisher>();

        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null && x.RetryCount < 20)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var integrationEvent = JsonSerializer.Deserialize<PartnerTransactionQueued>(message.Payload)
                    ?? throw new InvalidOperationException("Outbox payload could not be deserialized.");

                await publisher.PublishAsync(
                    integrationEvent.TransactionId.ToString("N"),
                    message.Payload,
                    cancellationToken);

                var transaction = await dbContext.PartnerTransactions
                    .SingleAsync(x => x.Id == integrationEvent.TransactionId, cancellationToken);
                transaction.MarkPublished();
                message.MarkProcessed(timeProvider.GetUtcNow());
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.MarkFailed(exception.Message);
                logger.LogWarning(exception, "Failed to publish outbox message {MessageId}", message.Id);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
