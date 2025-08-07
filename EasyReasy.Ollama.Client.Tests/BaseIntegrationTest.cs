using EasyReasy.Ollama.Server;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EasyReasy.Ollama.Client.Tests
{
    /// <summary>
    /// Base class for all integration tests that provides common setup and cleanup.
    /// </summary>
    public abstract class BaseIntegrationTest
    {
        protected static WebApplicationFactory<Program> Factory = null!;
        protected static HttpClient ServerClient = null!;
        protected OllamaClient? Client;

        protected const string chatTestModelName = "gemma3:1b";
        protected const string embeddingModelName = "nomic-embed-text";

        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void ClassSetup(TestContext context)
        {
            // Add test environment variables - must be set before creating the WebApplicationFactory
            Environment.SetEnvironmentVariable("OLLAMA_URL", "http://localhost:11434");
            Environment.SetEnvironmentVariable("TENANT_INFO_01", "test-tenant,test-api-key");
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_01", chatTestModelName);
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_02", embeddingModelName);
            Environment.SetEnvironmentVariable("JWT_SIGNING_SECRET", "this-is-a-test-signing-secret-that-should-be-replaced-if-this-is-hosted-for-real");

            Factory = new WebApplicationFactory<Program>();
            ServerClient = Factory.CreateClient();
        }

        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void ClassCleanup()
        {
            ServerClient?.Dispose();
            Factory?.Dispose();
        }

        [TestInitialize]
        public void Setup()
        {
            // Test-specific setup can go here if needed in the future
        }

        [TestCleanup]
        public void Cleanup()
        {
            Client?.Dispose();
        }

        /// <summary>
        /// Creates an authenticated client for testing.
        /// </summary>
        /// <returns>An authenticated OllamaClient instance.</returns>
        protected static async Task<OllamaClient> CreateAuthenticatedClientAsync()
        {
            if (ServerClient == null)
            {
                throw new InvalidOperationException("ServerClient is null. ClassInitialize may not have been called properly.");
            }
            return await OllamaClient.CreateAuthorizedAsync(ServerClient, "test-api-key");
        }

        /// <summary>
        /// Creates an unauthenticated client for testing.
        /// </summary>
        /// <returns>An unauthenticated OllamaClient instance.</returns>
        protected static OllamaClient CreateUnauthenticatedClient()
        {
            if (ServerClient == null)
            {
                throw new InvalidOperationException("ServerClient is null. ClassInitialize may not have been called properly.");
            }
            return OllamaClient.CreateUnauthorized(ServerClient, "test-api-key");
        }

        /// <summary>
        /// Creates an unauthenticated client with invalid credentials for testing authentication failures.
        /// </summary>
        /// <returns>An OllamaClient instance that will fail authentication.</returns>
        protected static OllamaClient CreateUnauthenticatedClientWithInvalidCredentials()
        {
            if (ServerClient == null)
            {
                throw new InvalidOperationException("ServerClient is null. ClassInitialize may not have been called properly.");
            }
            return OllamaClient.CreateUnauthorized(ServerClient, "invalid-api-key");
        }
    }
}