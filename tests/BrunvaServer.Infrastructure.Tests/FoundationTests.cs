using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using BrunvaServer.Application.Modules.Identity;

namespace BrunvaServer.Infrastructure.Tests;

[Collection(ApiCollection.Name)]
public sealed class FoundationTests(ApiFactory factory)
{
    [Fact]
    public async Task Organizations_are_isolated_and_refresh_reuse_revokes_the_family()
    {
        var ownerA = await SignInAsync($"a-{Guid.NewGuid():N}@example.com");
        var ownerB = await SignInAsync($"b-{Guid.NewGuid():N}@example.com");
        var organizationA = await CreateOrganizationAsync(ownerA, "Alpha");
        var organizationB = await CreateOrganizationAsync(ownerB, "Beta");

        var own = await ownerA.Client.GetAsync($"/api/v1/organizations/{organizationA}");
        var foreign = await ownerA.Client.GetAsync($"/api/v1/organizations/{organizationB}");
        var list = await ownerA.Client.GetFromJsonAsync<PageBody>("/api/v1/organizations");

        own.StatusCode.Should().Be(HttpStatusCode.OK);
        foreign.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        list!.Items.Select(item => item.Id).Should().Equal(organizationA);

        var refreshed = await RefreshAsync(ownerA.RefreshToken);
        var reused = await PostAsync("/api/v1/auth/refresh", new { refreshToken = ownerA.RefreshToken });
        var rotated = await PostAsync("/api/v1/auth/refresh", new { refreshToken = refreshed.RefreshToken });

        reused.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        rotated.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<Session> SignInAsync(string email)
    {
        var registered = await PostAsync("/api/v1/auth/register", new { email, password = "Password1", fullName = "Ada" });
        registered.StatusCode.Should().Be(HttpStatusCode.Created);
        var tokens = await LoginAsync(email);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return new Session(client, tokens.RefreshToken);
    }

    private async Task<AuthTokens> LoginAsync(string email)
    {
        var response = await PostAsync("/api/v1/auth/login", new { email, password = "Password1" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AuthTokens>())!;
    }

    private async Task<AuthTokens> RefreshAsync(string refreshToken)
    {
        var response = await PostAsync("/api/v1/auth/refresh", new { refreshToken });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AuthTokens>())!;
    }

    private static async Task<Guid> CreateOrganizationAsync(Session session, string name)
    {
        var response = await session.Client.PostAsJsonAsync("/api/v1/organizations", new { name, businessType = "LegalEntity" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<OrganizationBody>();
        return body!.Id;
    }

    private Task<HttpResponseMessage> PostAsync(string path, object body) =>
        factory.CreateClient().PostAsJsonAsync(path, body);

    private sealed record Session(HttpClient Client, string RefreshToken);

    private sealed record OrganizationBody(Guid Id);

    private sealed record PageBody(IReadOnlyList<OrganizationBody> Items, int PageNumber, int PageSize, int TotalCount);
}
