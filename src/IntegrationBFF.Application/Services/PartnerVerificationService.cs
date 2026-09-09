using IntegrationBFF.Application.Abstractions.Partners;
using IntegrationBFF.Application.Options;
using IntegrationBFF.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IntegrationBFF.Application.Services;

public sealed class PartnerVerificationService(
    IPartnerVerificationClient client,
    IOptions<PartnerVerificationOptions> options,
    ILogger<PartnerVerificationService> logger) : IPartnerVerificationService
{
    private readonly PartnerVerificationOptions _options = options.Value;

    public async Task<bool> IsVerifiedAsync(string partnerId, CancellationToken cancellationToken)
    {
        var maxAttempts = Math.Max(1, _options.MaxAttempts);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var result = await client.VerifyAsync(partnerId, cancellationToken);
                return result.IsVerified;
            }
            catch (Exception exception) when (IsTransient(exception, cancellationToken))
            {
                lastException = exception;
                logger.LogWarning(exception,
                    "Partner verification attempt {Attempt}/{MaxAttempts} failed for {PartnerId}",
                    attempt,
                    maxAttempts,
                    partnerId);

                if (attempt < maxAttempts)
                {
                    var delay = TimeSpan.FromMilliseconds(
                        Math.Max(0, _options.BaseDelayMilliseconds) * Math.Pow(2, attempt - 1));
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        throw new PartnerVerificationUnavailableException(partnerId, lastException!);
    }

    private static bool IsTransient(Exception exception, CancellationToken requestCancellationToken) => exception switch
    {
        TimeoutException => true,
        HttpRequestException => true,
        TaskCanceledException when !requestCancellationToken.IsCancellationRequested => true,
        _ => false
    };
}
