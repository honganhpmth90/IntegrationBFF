namespace IntegrationBFF.Application.Options;

public sealed class PartnerVerificationOptions
{
    public const string SectionName = "PartnerVerification";

    public string BaseUrl { get; init; } = "http://localhost:5080";
    public int MaxAttempts { get; init; } = 3;
    public int BaseDelayMilliseconds { get; init; } = 100;
    public int TimeoutMilliseconds { get; init; } = 1000;
}
