using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace EasyReasy.Ollama.Client.Tests
{
    /// <summary>
    /// Base class for all integration tests that provides common setup and cleanup.
    /// </summary>
    public abstract class BaseIntegrationTest
    {
        protected WebApplicationFactory<Program> Factory = null!;
        protected HttpClient ServerClient = null!;
        protected string BaseUrl = null!;
        protected OllamaClient? Client;

        [TestInitialize]
        public void Setup()
        {
            // Add test environment variables
            Environment.SetEnvironmentVariable("OLLAMA_URL", "http://localhost:11434");
            Environment.SetEnvironmentVariable("TENANT_INFO_01", "test-tenant,test-api-key");
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_01", "llama3.1");
            Environment.SetEnvironmentVariable("JWT_SIGNING_SECRET", "this-is-a-test-signing-secret-that-should-be-replaced-if-this-is-hosted-for-real");

            Factory = new WebApplicationFactory<Program>();
            ServerClient = Factory.CreateClient();
            BaseUrl = ServerClient.BaseAddress?.ToString() ?? "http://localhost";
        }

        [TestCleanup]
        public void Cleanup()
        {
            Client?.Dispose();
            ServerClient?.Dispose();
            Factory?.Dispose();
        }

        /// <summary>
        /// Creates an authenticated client for testing.
        /// </summary>
        /// <returns>An authenticated OllamaClient instance.</returns>
        protected async Task<OllamaClient> CreateAuthenticatedClientAsync()
        {
            Client = await OllamaClient.CreateAuthorizedAsync(BaseUrl, "test-api-key");
            return Client;
        }

        /// <summary>
        /// Creates an unauthenticated client for testing.
        /// </summary>
        /// <returns>An unauthenticated OllamaClient instance.</returns>
        protected OllamaClient CreateUnauthenticatedClient()
        {
            Client = OllamaClient.CreateAsync(BaseUrl, "test-api-key");
            return Client;
        }
    }
}