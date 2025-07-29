using EasyReasy.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class AuthenticationIntegrationTests
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        [TestInitialize]
        public void Setup()
        {
            // Add test environment variables
            Environment.SetEnvironmentVariable("OLLAMA_URL", "http://localhost:11434");
            Environment.SetEnvironmentVariable("TENANT_INFO_01", "test-tenant,test-api-key");
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_01", "llama3.1");
            Environment.SetEnvironmentVariable("JWT_SIGNING_SECRET", "this-is-a-test-signing-secret-that-should-be-replaced-if-this-is-hosted-for-real");

            // Debug: Print environment variables
            Console.WriteLine($"JWT_SIGNING_SECRET: {Environment.GetEnvironmentVariable("JWT_SIGNING_SECRET")}");
            Console.WriteLine($"TENANT_INFO_01: {Environment.GetEnvironmentVariable("TENANT_INFO_01")}");

            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task AuthenticateWithApiKey_ValidApiKey_ReturnsJwtToken()
        {
            // Arrange
            object request = new { apiKey = "test-api-key" };
            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/auth/apikey", content);

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success status code, but got {response.StatusCode}");

            string responseContent = await response.Content.ReadAsStringAsync();
            AuthResponse? authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(authResponse);
            Assert.IsFalse(string.IsNullOrEmpty(authResponse.Token));
            Assert.IsFalse(string.IsNullOrEmpty(authResponse.ExpiresAt));

            // Debug: Print token details
            Console.WriteLine($"Token: {authResponse.Token}");
            Console.WriteLine($"ExpiresAt: {authResponse.ExpiresAt}");
        }

        [TestMethod]
        public async Task AuthenticateWithApiKey_InvalidApiKey_ReturnsUnauthorized()
        {
            // Arrange
            object request = new { apiKey = "invalid-api-key" };
            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/auth/apikey", content);

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamSse_WithValidToken_ReturnsSuccess()
        {
            // Arrange - First get a valid token
            object authRequest = new { apiKey = "test-api-key" };
            string authJson = JsonSerializer.Serialize(authRequest);
            StringContent authContent = new StringContent(authJson, Encoding.UTF8, "application/json");

            HttpResponseMessage authResponse = await _client.PostAsync("/api/auth/apikey", authContent);
            Assert.IsTrue(authResponse.IsSuccessStatusCode);

            string authResponseContent = await authResponse.Content.ReadAsStringAsync();
            AuthResponse? authResult = JsonSerializer.Deserialize<AuthResponse>(authResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Debug: Print token details
            Console.WriteLine($"Token for stream test: {authResult!.Token}");

            // Act - Use the token to access the stream endpoint
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
            HttpResponseMessage streamResponse = await _client.GetAsync("/stream-sse?text=Hello&model=llama3.1");

            // Debug: Print response details
            Console.WriteLine($"Stream response status: {streamResponse.StatusCode}");
            string streamResponseContent = await streamResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Stream response content: {streamResponseContent}");

            // Assert
            Assert.IsTrue(streamResponse.IsSuccessStatusCode, $"Expected success status code, but got {streamResponse.StatusCode}");
            Assert.AreEqual("text/event-stream", streamResponse.Content.Headers.ContentType?.MediaType);
        }

        [TestMethod]
        public async Task StreamSse_WithValidTokenButInvalidModel_ReturnsReadableError()
        {
            // Arrange - First get a valid token
            object authRequest = new { apiKey = "test-api-key" };
            string authJson = JsonSerializer.Serialize(authRequest);
            StringContent authContent = new StringContent(authJson, Encoding.UTF8, "application/json");

            HttpResponseMessage authResponse = await _client.PostAsync("/api/auth/apikey", authContent);
            Assert.IsTrue(authResponse.IsSuccessStatusCode);

            string authResponseContent = await authResponse.Content.ReadAsStringAsync();
            AuthResponse? authResult = JsonSerializer.Deserialize<AuthResponse>(authResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Debug: Print token details
            Console.WriteLine($"Token for stream test: {authResult!.Token}");

            // Act - Use the token to access the stream endpoint
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
            HttpResponseMessage streamResponse = await _client.GetAsync("/stream-sse?text=Hello&model=fakemodel");

            // Debug: Print response details
            Console.WriteLine($"Stream response status: {streamResponse.StatusCode}");
            string streamResponseContent = await streamResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Stream response content: {streamResponseContent}");

            // Assert
            Assert.IsFalse(streamResponse.IsSuccessStatusCode, $"Expected failure status code, but got {streamResponse.StatusCode}");
            Assert.AreEqual("text/event-stream", streamResponse.Content.Headers.ContentType?.MediaType);
        }

        [TestMethod]
        public async Task StreamSse_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/stream-sse?text=Hello");

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private class DebugAuthResponse
        {
            public bool IsAuthenticated { get; set; }
            public string? UserId { get; set; }
            public string? TenantId { get; set; }
            public string? AuthType { get; set; }
            public List<ClaimInfo> Claims { get; set; } = new List<ClaimInfo>();
        }

        private class ClaimInfo
        {
            public string Type { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
    }
}