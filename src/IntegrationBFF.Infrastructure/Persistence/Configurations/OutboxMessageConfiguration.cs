using IntegrationBFF.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegrationBFF.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("longtext").IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc });
    }
}
