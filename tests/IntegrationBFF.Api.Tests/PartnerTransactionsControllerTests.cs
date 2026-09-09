using IntegrationBFF.Api.Controllers;
using IntegrationBFF.Application.Transactions.Commands;
using IntegrationBFF.Application.Transactions.Contracts;
using IntegrationBFF.Application.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace IntegrationBFF.Api.Tests;

[TestFixture]
public sealed class PartnerTransactionsControllerTests
{
    [Test]
    public async Task Create_MapsRequestToCommandAndReturnsAccepted()
    {
        var sender = Substitute.For<ISender>();
        var id = Guid.NewGuid();
        sender.Send(Arg.Any<CreatePartnerTransactionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new TransactionAcceptedResponse(id, "PendingPublication"));
        var controller = new PartnerTransactionsController(sender);
        var request = new PartnerTransactionRequest(
            "P-1001", "TXN-99823", 250m, "USD", DateTimeOffset.Parse("2024-05-10T14:30:00Z"));

        var result = await controller.Create(request, CancellationToken.None);

        var accepted = result.Result as AcceptedAtActionResult;
        Assert.That(accepted, Is.Not.Null);
        Assert.That(accepted!.StatusCode, Is.EqualTo(202));
        Assert.That(accepted.Value, Is.EqualTo(new TransactionAcceptedResponse(id, "PendingPublication")));
        await sender.Received(1).Send(
            Arg.Is<CreatePartnerTransactionCommand>(command =>
                command.PartnerId == request.PartnerId &&
                command.TransactionReference == request.TransactionReference &&
                command.Amount == request.Amount &&
                command.Currency == request.Currency &&
                command.Timestamp == request.Timestamp),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetById_ReturnsQueryResult()
    {
        var sender = Substitute.For<ISender>();
        var id = Guid.NewGuid();
        var response = new TransactionDetailsResponse(
            id, "P-1001", "TXN-99823", 250m, "USD", DateTimeOffset.UtcNow, "Published");
        sender.Send(Arg.Any<GetPartnerTransactionQuery>(), Arg.Any<CancellationToken>()).Returns(response);
        var controller = new PartnerTransactionsController(sender);

        var result = await controller.GetById(id, CancellationToken.None);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(response));
        await sender.Received(1).Send(
            Arg.Is<GetPartnerTransactionQuery>(query => query.TransactionId == id),
            Arg.Any<CancellationToken>());
    }
}
