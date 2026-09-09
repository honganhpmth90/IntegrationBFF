using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Application.Transactions.Queries;
using IntegrationBFF.Core.Exceptions;
using IntegrationBFF.Domain.Transactions;
using NSubstitute;

namespace IntegrationBFF.Application.Tests;

[TestFixture]
public sealed class GetPartnerTransactionQueryHandlerTests
{
    [Test]
    public async Task Handle_WhenTransactionExists_MapsDetails()
    {
        var repository = Substitute.For<ITransactionRepository>();
        var transaction = PartnerTransaction.Create(
            "P-1001", "TXN-99823", 250m, "usd", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        var handler = new GetPartnerTransactionQueryHandler(repository);

        var response = await handler.Handle(new GetPartnerTransactionQuery(transaction.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.TransactionId, Is.EqualTo(transaction.Id));
            Assert.That(response.PartnerId, Is.EqualTo("P-1001"));
            Assert.That(response.TransactionReference, Is.EqualTo("TXN-99823"));
            Assert.That(response.Amount, Is.EqualTo(250m));
            Assert.That(response.Currency, Is.EqualTo("USD"));
            Assert.That(response.Status, Is.EqualTo("PendingPublication"));
        });
    }

    [Test]
    public void Handle_WhenTransactionDoesNotExist_ThrowsNotFound()
    {
        var repository = Substitute.For<ITransactionRepository>();
        var id = Guid.NewGuid();
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((PartnerTransaction?)null);
        var handler = new GetPartnerTransactionQueryHandler(repository);

        Assert.That(
            async () => await handler.Handle(new GetPartnerTransactionQuery(id), CancellationToken.None),
            Throws.TypeOf<TransactionNotFoundException>());
    }
}
