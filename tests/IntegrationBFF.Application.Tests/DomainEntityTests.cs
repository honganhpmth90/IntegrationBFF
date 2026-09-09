using IntegrationBFF.Domain.Outbox;
using IntegrationBFF.Domain.Transactions;

namespace IntegrationBFF.Application.Tests;

[TestFixture]
public sealed class DomainEntityTests
{
    [Test]
    public void PartnerTransaction_CreateNormalizesCurrency_AndCanBePublished()
    {
        var transaction = PartnerTransaction.Create(
            "P-1001", "TXN-99823", 250m, "usd", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        Assert.That(transaction.Currency, Is.EqualTo("USD"));
        Assert.That(transaction.Status, Is.EqualTo(TransactionStatus.PendingPublication));

        transaction.MarkPublished();

        Assert.That(transaction.Status, Is.EqualTo(TransactionStatus.Published));
    }

    [Test]
    public void OutboxMessage_TracksFailuresAndSuccessfulProcessing()
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var message = OutboxMessage.Create("PartnerTransactionQueued", "{}", occurredAt);

        message.MarkFailed(new string('x', 2100));

        Assert.That(message.RetryCount, Is.EqualTo(1));
        Assert.That(message.LastError, Has.Length.EqualTo(2000));

        var processedAt = occurredAt.AddSeconds(1);
        message.MarkProcessed(processedAt);

        Assert.That(message.ProcessedAtUtc, Is.EqualTo(processedAt));
        Assert.That(message.LastError, Is.Null);
    }
}
