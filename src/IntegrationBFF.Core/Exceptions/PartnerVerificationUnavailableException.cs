namespace IntegrationBFF.Core.Exceptions;

public sealed class PartnerVerificationUnavailableException(string partnerId, Exception innerException)
    : IntegrationBffException($"Partner verification is temporarily unavailable for '{partnerId}'.", innerException);
