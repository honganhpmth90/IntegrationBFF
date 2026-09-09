using FluentValidation;
using IntegrationBFF.Application.Abstractions.Partners;
using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Application.Transactions.Commands;
using IntegrationBFF.Core.Exceptions;
using IntegrationBFF.Domain.Outbox;
using IntegrationBFF.Domain.Transactions;
using NSubstitute;

namespace IntegrationBFF.Application.Tests;

[TestFixture]
public sealed class CreatePartnerTransactionCommandHandlerTests
{
    [Test]
    public async Task Handle_WithVerifiedPartner_PersistsTransactionAndOutboxAtomically()
    {
        var dependencies = CreateDependencies();
        dependencies.Verification.IsVerifiedAsync("P-1001", Arg.Any<CancellationToken>()).Returns(true);
        var handler = dependencies.CreateHandler();

        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(response.TransactionId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(response.Status, Is.EqualTo(TransactionStatus.PendingPublication.ToString()));
        await dependencies.Transactions.Received(1)
            .AddAsync(Arg.Any<PartnerTransaction>(), Arg.Any<CancellationToken>());
        await dependencies.Outbox.Received(1)
            .AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
        await dependencies.UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WithDuplicateReference_ReturnsExistingWithoutVerificationOrWrites()
    {
        var dependencies = CreateDependencies();
        var existing = PartnerTransaction.Create(
            "P-1001", "TXN-99823", 250m, "USD", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        dependencies.Transactions
            .GetByReferenceAsync("P-1001", "TXN-99823", Arg.Any<CancellationToken>())
            .Returns(existing);
        var handler = dependencies.CreateHandler();

        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(response.TransactionId, Is.EqualTo(existing.Id));
        Assert.That(response.IsDuplicate, Is.True);
        await dependencies.Verification.DidNotReceiveWithAnyArgs()
            .IsVerifiedAsync(default!, default);
        await dependencies.UnitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Test]
    public void Handle_WithInvalidPayload_StopsBeforeExternalCalls()
    {
        var dependencies = CreateDependencies();
        var handler = dependencies.CreateHandler();
        var invalidCommand = ValidCommand() with { Amount = 0m };

        Assert.That(
            async () => await handler.Handle(invalidCommand, CancellationToken.None),
            Throws.TypeOf<ValidationException>());

        dependencies.Verification.DidNotReceiveWithAnyArgs().IsVerifiedAsync(default!, default);
    }

    [Test]
    public void Handle_WithUnverifiedPartner_ThrowsAndDoesNotPersist()
    {
        var dependencies = CreateDependencies();
        dependencies.Verification.IsVerifiedAsync("P-1001", Arg.Any<CancellationToken>()).Returns(false);
        var handler = dependencies.CreateHandler();

        Assert.That(
            async () => await handler.Handle(ValidCommand(), CancellationToken.None),
            Throws.TypeOf<PartnerNotVerifiedException>());

        dependencies.Transactions.DidNotReceiveWithAnyArgs()
            .AddAsync(default!, default);
        dependencies.Outbox.DidNotReceiveWithAnyArgs()
            .AddAsync(default!, default);
        dependencies.UnitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    private static Dependencies CreateDependencies() => new(
        Substitute.For<IPartnerVerificationService>(),
        Substitute.For<ITransactionRepository>(),
        Substitute.For<IOutboxRepository>(),
        Substitute.For<IUnitOfWork>());

    private static CreatePartnerTransactionCommand ValidCommand() =>
        new("P-1001", "TXN-99823", 250m, "USD", DateTimeOffset.Parse("2024-05-10T14:30:00Z"));

    private sealed record Dependencies(
        IPartnerVerificationService Verification,
        ITransactionRepository Transactions,
        IOutboxRepository Outbox,
        IUnitOfWork UnitOfWork)
    {
        public CreatePartnerTransactionCommandHandler CreateHandler() =>
            new(
                new CreatePartnerTransactionCommandValidator(),
                Verification,
                Transactions,
                Outbox,
                UnitOfWork,
                TimeProvider.System);
    }
}
