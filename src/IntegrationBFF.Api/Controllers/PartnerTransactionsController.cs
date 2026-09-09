using IntegrationBFF.Application.Transactions.Commands;
using IntegrationBFF.Application.Transactions.Contracts;
using IntegrationBFF.Application.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationBFF.Api.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
public sealed class PartnerTransactionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "PartnerTransactions.Write")]
    [ProducesResponseType<TransactionAcceptedResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TransactionAcceptedResponse>> Create(
        [FromBody] PartnerTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePartnerTransactionCommand(
            request.PartnerId,
            request.TransactionReference,
            request.Amount,
            request.Currency,
            request.Timestamp);

        var result = await sender.Send(command, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { transactionId = result.TransactionId }, result);
    }

    [HttpGet("{transactionId:guid}")]
    [Authorize(Policy = "PartnerTransactions.Read")]
    [ProducesResponseType<TransactionDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDetailsResponse>> GetById(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPartnerTransactionQuery(transactionId), cancellationToken);
        return Ok(result);
    }
}
