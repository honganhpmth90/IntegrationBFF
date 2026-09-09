using IntegrationBFF.Application.Abstractions.Partners;
using IntegrationBFF.Application.Options;
using IntegrationBFF.Application.Services;
using IntegrationBFF.Core.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IntegrationBFF.Application.Tests;

[TestFixture]
public sealed class PartnerVerificationServiceTests
{
    [Test]
    public async Task IsVerifiedAsync_AfterTransientFailures_RetriesAndReturnsResult()
    {
        var client = Substitute.For<IPartnerVerificationClient>();
        client.VerifyAsync("P-1001", Arg.Any<CancellationToken>()).Returns(
            _ => Task.FromException<PartnerVerificationResult>(new TimeoutException()),
            _ => Task.FromException<PartnerVerificationResult>(new HttpRequestException()),
            _ => Task.FromResult(new PartnerVerificationResult(true)));
        var service = CreateService(client, maxAttempts: 3);

        var result = await service.IsVerifiedAsync("P-1001", CancellationToken.None);

        Assert.That(result, Is.True);
        await client.Received(3).VerifyAsync("P-1001", Arg.Any<CancellationToken>());
    }

    [Test]
    public void IsVerifiedAsync_WhenRetriesExhausted_ThrowsUnavailableException()
    {
        var client = Substitute.For<IPartnerVerificationClient>();
        client.VerifyAsync("P-1001", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<PartnerVerificationResult>(new TimeoutException()));
        var service = CreateService(client, maxAttempts: 3);

        Assert.That(
            async () => await service.IsVerifiedAsync("P-1001", CancellationToken.None),
            Throws.TypeOf<PartnerVerificationUnavailableException>());

        client.Received(3).VerifyAsync("P-1001", Arg.Any<CancellationToken>());
    }

    [Test]
    public void IsVerifiedAsync_WithNonTransientFailure_DoesNotRetry()
    {
        var client = Substitute.For<IPartnerVerificationClient>();
        client.VerifyAsync("P-1001", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<PartnerVerificationResult>(new InvalidOperationException("bad response")));
        var service = CreateService(client, maxAttempts: 3);

        Assert.That(
            async () => await service.IsVerifiedAsync("P-1001", CancellationToken.None),
            Throws.TypeOf<InvalidOperationException>());

        client.Received(1).VerifyAsync("P-1001", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task IsVerifiedAsync_WhenPartnerIsInvalid_ReturnsFalseWithoutRetry()
    {
        var client = Substitute.For<IPartnerVerificationClient>();
        client.VerifyAsync("unknown", Arg.Any<CancellationToken>())
            .Returns(new PartnerVerificationResult(false));
        var service = CreateService(client, maxAttempts: 3);

        var result = await service.IsVerifiedAsync("unknown", CancellationToken.None);

        Assert.That(result, Is.False);
        await client.Received(1).VerifyAsync("unknown", Arg.Any<CancellationToken>());
    }

    private static PartnerVerificationService CreateService(
        IPartnerVerificationClient client,
        int maxAttempts) =>
        new(
            client,
            Microsoft.Extensions.Options.Options.Create(new PartnerVerificationOptions
            {
                MaxAttempts = maxAttempts,
                BaseDelayMilliseconds = 0
            }),
            NullLogger<PartnerVerificationService>.Instance);
}
