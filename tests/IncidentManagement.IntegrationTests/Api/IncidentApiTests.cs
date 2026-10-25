using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Common;
using IncidentManagement.Domain.Enums;
using IncidentManagement.IntegrationTests.Infrastructure;

namespace IncidentManagement.IntegrationTests.Api;

public class IncidentApiTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IncidentApiTests(IntegrationTestWebAppFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Viewer_CannotCreateIncident()
    {
        await AuthenticateAsync("viewer@incident.local", "Viewer123!");
        var response = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            "API outage", "Customers cannot trade", IncidentSeverity.SEV2, null));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Responder_CanCreateAndTransitionIncident()
    {
        await AuthenticateAsync("responder@incident.local", "Responder123!");

        var createResponse = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            "Database latency", "Elevated query times in EU", IncidentSeverity.SEV3, null));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<IncidentDetailDto>(JsonOptions);
        created.Should().NotBeNull();
        created!.Status.Should().Be(IncidentStatus.Open);

        var transitionResponse = await _client.PostAsJsonAsync(
            $"/api/incidents/{created.Id}/status",
            new TransitionStatusRequest(IncidentStatus.Acknowledged, "On it"));
        transitionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await transitionResponse.Content.ReadFromJsonAsync<IncidentDetailDto>(JsonOptions);
        updated!.Status.Should().Be(IncidentStatus.Acknowledged);
        updated.StatusAudits.Should().ContainSingle(a => a.ToStatus == IncidentStatus.Acknowledged);
    }

    [Fact]
    public async Task Admin_CanListUsers()
    {
        await AuthenticateAsync("admin@incident.local", "Admin123!");
        var response = await _client.GetAsync("/api/auth/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(JsonOptions);
        users.Should().NotBeNull().And.HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Incidents_ListSupportsPaging()
    {
        await AuthenticateAsync("responder@incident.local", "Responder123!");
        var response = await _client.GetAsync("/api/incidents?page=1&pageSize=5&sortBy=createdAt&sortDirection=desc");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<IncidentDto>>(JsonOptions);
        page.Should().NotBeNull();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(5);
    }

    private async Task AuthenticateAsync(string email, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }
}
