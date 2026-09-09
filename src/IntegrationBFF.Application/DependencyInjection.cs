using FluentValidation;
using IntegrationBFF.Application.Abstractions.Partners;
using IntegrationBFF.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationBFF.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IPartnerVerificationService, PartnerVerificationService>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
