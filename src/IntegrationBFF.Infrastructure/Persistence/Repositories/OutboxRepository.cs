using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Domain.Outbox;

namespace IntegrationBFF.Infrastructure.Persistence.Repositories;

internal sealed class OutboxRepository(IntegrationBffDbContext dbContext) : IOutboxRepository
{
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken) =>
        dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();
}
