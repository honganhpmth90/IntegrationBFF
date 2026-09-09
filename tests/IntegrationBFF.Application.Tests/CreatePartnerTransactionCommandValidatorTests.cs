using IntegrationBFF.Application.Transactions.Commands;

namespace IntegrationBFF.Application.Tests;

[TestFixture]
public sealed class CreatePartnerTransactionCommandValidatorTests
{
    private readonly CreatePartnerTransactionCommandValidator _validator = new();

    [Test]
    public async Task Validate_WithValidPayload_IsValid()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        Assert.That(result.IsValid, Is.True);
    }

    [TestCase(0)]
    [TestCase(-0.01)]
    public async Task Validate_WithNonPositiveAmount_IsInvalid(decimal amount)
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { Amount = amount });

        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("Amount"));
    }

    [TestCase("")]
    [TestCase("ZZZ")]
    [TestCase("US")]
    public async Task Validate_WithInvalidCurrency_IsInvalid(string currency)
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { Currency = currency });

        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("Currency"));
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task Validate_WithMissingPartnerId_IsInvalid(string partnerId)
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { PartnerId = partnerId });

        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("PartnerId"));
    }

    [Test]
    public async Task Validate_WithDefaultTimestamp_IsInvalid()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { Timestamp = default });

        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("Timestamp"));
    }

    private static CreatePartnerTransactionCommand ValidCommand() =>
        new("P-1001", "TXN-99823", 250m, "USD", DateTimeOffset.Parse("2024-05-10T14:30:00Z"));
}
