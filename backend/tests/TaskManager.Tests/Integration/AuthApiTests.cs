using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManager.Application.Auth;
using TaskManager.Application.Users;

namespace TaskManager.Tests.Integration;

[Collection(ApiCollection.Name)]
public sealed class AuthApiTests(ApiFactory factory)
{
    [Fact]
    public async Task Register_ThenLogin_ReturnsUsableToken()
    {
        var client = factory.CreateClient();
        var email = $"Alice-{Guid.NewGuid():N}@Example.com";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Alice", email, "password123"), ApiClient.Json);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email.ToUpperInvariant(), "password123"), ApiClient.Json);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var auth = await login.ReadAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var me = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var user = await me.ReadAsync<UserDto>();
        Assert.Equal("Alice", user.Name);
        Assert.Equal(email.ToLowerInvariant(), user.Email);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        var client = factory.CreateClient();
        var request = new RegisterRequest("Bob", $"bob-{Guid.NewGuid():N}@example.com", "password123");

        await client.PostAsJsonAsync("/api/auth/register", request, ApiClient.Json);
        var second = await client.PostAsJsonAsync("/api/auth/register", request, ApiClient.Json);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Theory]
    [InlineData("", "valid@example.com", "password123")]
    [InlineData("Name", "invalid-email", "password123")]
    [InlineData("Name", "valid@example.com", "short")]
    public async Task Register_WithInvalidData_ReturnsBadRequest(string name, string email, string password)
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/register", new RegisterRequest(name, email, password), ApiClient.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var (_, auth) = await ApiClient.RegisterAsync(factory);

        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new LoginRequest(auth.User.Email, "wrong-password"), ApiClient.Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesCurrentToken()
    {
        var (client, _) = await ApiClient.RegisterAsync(factory);

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await factory.CreateClient().GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReportsReady()
    {
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
    }
}
