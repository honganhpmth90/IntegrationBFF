using FluentValidation;
using System.Globalization;

namespace IntegrationBFF.Application.Transactions.Commands;

public sealed class CreatePartnerTransactionCommandValidator : AbstractValidator<CreatePartnerTransactionCommand>
{
    private static readonly HashSet<string> Iso4217Currencies = CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(culture => new RegionInfo(culture.Name).ISOCurrencySymbol)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public CreatePartnerTransactionCommandValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.TransactionReference)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Must(currency => Iso4217Currencies.Contains(currency))
            .WithMessage("Currency must be a supported ISO 4217 code.");

        RuleFor(x => x.Timestamp)
            .NotEqual(default(DateTimeOffset));
    }
}
