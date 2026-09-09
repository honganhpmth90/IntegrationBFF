namespace IntegrationBFF.Core.Exceptions;

public sealed class PartnerNotVerifiedException(string partnerId)
    : IntegrationBffException($"Partner '{partnerId}' could not be verified.");
