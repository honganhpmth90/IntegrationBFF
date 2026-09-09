using System.Net;
using System.Net.Http.Json;
using IntegrationBFF.Application.Abstractions.Partners;

namespace IntegrationBFF.Infrastructure.Partners;

internal sealed class PartnerVerificationHttpClient(HttpClient httpClient) : IPartnerVerificationClient
{
    public async Task<PartnerVerificationResult> VerifyAsync(
        string partnerId,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"api/v1/mock/partners/{Uri.EscapeDataString(partnerId)}/verify",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.GatewayTimeout)
        {
            throw new TimeoutException("The partner verification API timed out.");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PartnerVerificationResult>(cancellationToken)
            ?? throw new HttpRequestException("Partner verification returned an empty response.");
    }
}
