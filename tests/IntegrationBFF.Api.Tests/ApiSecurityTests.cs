using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IntegrationBFF.Application.Transactions.Commands;
using IntegrationBFF.Application.Transactions.Contracts;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace IntegrationBFF.Api.Tests;

[TestFixture]
public sealed class ApiSecurityTests
{
    private const string SigningKey = "test-signing-key-with-at-least-thirty-two-bytes";
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable(
            "Jwt__SigningKey",
            SigningKey);
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHostedService>();
                services.RemoveAll<ISender>();
                var sender = Substitute.For<ISender>();
                sender.Send(Arg.Any<CreatePartnerTransactionCommand>(), Arg.Any<CancellationToken>())
                    .Returns(new TransactionAcceptedResponse(Guid.NewGuid(), "PendingPublication"));
                services.AddSingleton(sender);
            });
        });
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
        Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
    }

    [Test]
    public async Task HealthEndpoint_IsAnonymous()
    {
        using var response = await _client.GetAsync("/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PartnerTransactionEndpoint_WithoutJwt_ReturnsUnauthorized()
    {
        var request = new PartnerTransactionRequest(
            "P-1001",
            "TXN-99823",
            250m,
            "USD",
            DateTimeOffset.Parse("2024-05-10T14:30:00Z"));

        using var response = await _client.PostAsJsonAsync("/api/v1/partner/transactions", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task PartnerTransactionEndpoint_WithWrongScope_ReturnsForbidden()
    {
        using var request = CreateRequest("unrelated.scope");

        using var response = await _client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task PartnerTransactionEndpoint_WithSpaceSeparatedWriteScope_ReturnsAccepted()
    {
        using var request = CreateRequest("partner.transactions.read partner.transactions.write");

        using var response = await _client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
    }

    private static HttpRequestMessage CreateRequest(string scopes)
    {
        var transaction = new PartnerTransactionRequest(
            "P-1001",
            "TXN-99823",
            250m,
            "USD",
            DateTimeOffset.Parse("2024-05-10T14:30:00Z"));
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/partner/transactions")
        {
            Content = JsonContent.Create(transaction)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(scopes));
        return request;
    }

    private static string CreateJwt(string scopes)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "integration-bff",
            audience: "partner-api",
            claims: [new Claim("scope", scopes)],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: signingCredentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
