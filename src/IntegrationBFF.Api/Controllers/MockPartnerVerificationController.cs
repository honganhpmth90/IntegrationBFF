using IntegrationBFF.Application.Abstractions.Partners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationBFF.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/mock/partners")]
public sealed class MockPartnerVerificationController : ControllerBase
{
    [HttpGet("{partnerId}/verify")]
    [ProducesResponseType<PartnerVerificationResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public ActionResult<PartnerVerificationResult> Verify(string partnerId)
    {
        if (Random.Shared.NextDouble() < 0.30)
        {
            throw new TimeoutException("Simulated partner verification timeout.");
        }

        var isVerified = partnerId.StartsWith("P-", StringComparison.OrdinalIgnoreCase);
        return Ok(new PartnerVerificationResult(isVerified));
    }
}
