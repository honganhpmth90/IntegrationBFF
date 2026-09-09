using System.Text.Json;
using FluentValidation;
using IntegrationBFF.Application.Abstractions.Partners;
using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Application.Transactions.Contracts;
using IntegrationBFF.Core.Exceptions;
using IntegrationBFF.Domain.Outbox;
using IntegrationBFF.Domain.Transactions;
using MediatR;

namespace IntegrationBFF.Application.Transactions.Commands;

public sealed class CreatePartnerTransactionCommandHandler(
    IValidator<CreatePartnerTransactionCommand> validator,
    IPartnerVerificationService partnerVerificationService,
    ITransactionRepository transactionRepository,
    IOutboxRepository outboxRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CreatePartnerTransactionCommand, TransactionAcceptedResponse>
{
    public async Task<TransactionAcceptedResponse> Handle(
        CreatePartnerTransactionCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var existing = await transactionRepository.GetByReferenceAsync(
            request.PartnerId,
            request.TransactionReference,
            cancellationToken);

        if (existing is not null)
        {
            return new TransactionAcceptedResponse(existing.Id, existing.Status.ToString(), true);
        }

        if (!await partnerVerificationService.IsVerifiedAsync(request.PartnerId, cancellationToken))
        {
            throw new PartnerNotVerifiedException(request.PartnerId);
        }

        var now = timeProvider.GetUtcNow();
        var transaction = PartnerTransaction.Create(
            request.PartnerId,
            request.TransactionReference,
            request.Amount,
            request.Currency,
            request.Timestamp,
            now);

        var integrationEvent = new PartnerTransactionQueued(
            transaction.Id,
            transaction.PartnerId,
            transaction.TransactionReference,
            transaction.Amount,
            transaction.Currency,
            transaction.TransactionTimestamp,
            now);

        var outboxMessage = OutboxMessage.Create(
            nameof(PartnerTransactionQueued),
            JsonSerializer.Serialize(integrationEvent),
            now);

        await transactionRepository.AddAsync(transaction, cancellationToken);
        await outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransactionAcceptedResponse(transaction.Id, transaction.Status.ToString());
    }
}
