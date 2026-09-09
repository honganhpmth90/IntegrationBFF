using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Domain.Outbox;
using IntegrationBFF.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace IntegrationBFF.Infrastructure.Persistence;

public sealed class IntegrationBffDbContext(DbContextOptions<IntegrationBffDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<PartnerTransaction> PartnerTransactions => Set<PartnerTransaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationBffDbContext).Assembly);
    }
}
