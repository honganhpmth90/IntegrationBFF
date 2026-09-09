namespace IntegrationBFF.Application.Abstractions.Partners;

public interface IPartnerVerificationClient
{
    Task<PartnerVerificationResult> VerifyAsync(string partnerId, CancellationToken cancellationToken);
}

public sealed record PartnerVerificationResult(bool IsVerified);
