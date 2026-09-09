using IntegrationBFF.Domain.Outbox;

namespace IntegrationBFF.Application.Abstractions.Persistence;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);
}
