using IntegrationBFF.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegrationBFF.Infrastructure.Persistence.Configurations;

internal sealed class PartnerTransactionConfiguration : IEntityTypeConfiguration<PartnerTransaction>
{
    public void Configure(EntityTypeBuilder<PartnerTransaction> builder)
    {
        builder.ToTable("partner_transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PartnerId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TransactionReference).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(x => new { x.PartnerId, x.TransactionReference }).IsUnique();
    }
}
