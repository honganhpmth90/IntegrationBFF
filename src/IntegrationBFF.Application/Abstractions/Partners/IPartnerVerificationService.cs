namespace IntegrationBFF.Application.Abstractions.Partners;

public interface IPartnerVerificationService
{
    Task<bool> IsVerifiedAsync(string partnerId, CancellationToken cancellationToken);
}
