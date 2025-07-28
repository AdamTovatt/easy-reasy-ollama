using EasyReasy.Ollama.Server.Models.Tenants;
using EasyReasy.Ollama.Server.Services.Tenants;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class EnvironmentVariablesTenantServiceTests
    {
        private EnvironmentVariablesTenantService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            // Set up test environment variables
            Environment.SetEnvironmentVariable("TENANT_INFO1", "tenant1,api-key-1");
            Environment.SetEnvironmentVariable("TENANT_INFO2", "tenant2,api-key-2");
            Environment.SetEnvironmentVariable("TENANT_INFO_DEV", "dev-tenant,dev-api-key");
            Environment.SetEnvironmentVariable("TENANT_INFO_PROD", "prod-tenant,prod-api-key");

            _service = new EnvironmentVariablesTenantService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test environment variables
            Environment.SetEnvironmentVariable("TENANT_INFO1", null);
            Environment.SetEnvironmentVariable("TENANT_INFO2", null);
            Environment.SetEnvironmentVariable("TENANT_INFO_DEV", null);
            Environment.SetEnvironmentVariable("TENANT_INFO_PROD", null);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithValidApiKey_ReturnsTenantInfo()
        {
            // Arrange
            string validApiKey = "api-key-1";

            // Act
            TenantInfo? result = _service.GetTenantInfoByApiKey(validApiKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("tenant1", result.TenantId);
            Assert.AreEqual(validApiKey, result.ApiKey);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithAnotherValidApiKey_ReturnsCorrectTenantInfo()
        {
            // Arrange
            string validApiKey = "dev-api-key";

            // Act
            TenantInfo? result = _service.GetTenantInfoByApiKey(validApiKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("dev-tenant", result.TenantId);
            Assert.AreEqual(validApiKey, result.ApiKey);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithInvalidApiKey_ReturnsNull()
        {
            // Arrange
            string invalidApiKey = "non-existent-api-key";

            // Act
            TenantInfo? result = _service.GetTenantInfoByApiKey(invalidApiKey);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithNullApiKey_ReturnsNull()
        {
            // Act
            TenantInfo? result = _service.GetTenantInfoByApiKey(null!);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithEmptyApiKey_ReturnsNull()
        {
            // Act
            TenantInfo? result = _service.GetTenantInfoByApiKey("");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithWhitespaceApiKey_ReturnsNull()
        {
            // Act
            TenantInfo? result = _service.GetTenantInfoByApiKey("   ");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithCaseSensitiveApiKey_ReturnsCorrectTenantInfo()
        {
            // Arrange
            string validApiKey = "API-KEY-1"; // Different case than stored

            // Act
            TenantInfo? result = _service.GetTenantInfoByApiKey(validApiKey);

            // Assert
            Assert.IsNull(result, "API key lookup should be case-sensitive");
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithAllValidApiKeys_ReturnsCorrectTenantInfo()
        {
            // Test all configured API keys
            string[] validApiKeys = { "api-key-1", "api-key-2", "dev-api-key", "prod-api-key" };
            string[] expectedTenantIds = { "tenant1", "tenant2", "dev-tenant", "prod-tenant" };

            for (int i = 0; i < validApiKeys.Length; i++)
            {
                // Act
                TenantInfo? result = _service.GetTenantInfoByApiKey(validApiKeys[i]);

                // Assert
                Assert.IsNotNull(result, $"API key '{validApiKeys[i]}' should return tenant info");
                Assert.AreEqual(expectedTenantIds[i], result.TenantId, $"API key '{validApiKeys[i]}' should return correct tenant ID");
                Assert.AreEqual(validApiKeys[i], result.ApiKey, $"API key '{validApiKeys[i]}' should return correct API key");
            }
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithMalformedEnvironmentVariable_IgnoresInvalidEntry()
        {
            // Arrange - Add a malformed environment variable
            Environment.SetEnvironmentVariable("TENANT_INFO_MALFORMED", "invalid-format-without-comma");

            // Recreate service to pick up new environment variable
            _service = new EnvironmentVariablesTenantService();

            // Act - Should still work with valid API keys
            TenantInfo? result = _service.GetTenantInfoByApiKey("api-key-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("tenant1", result.TenantId);

            // Clean up
            Environment.SetEnvironmentVariable("TENANT_INFO_MALFORMED", null);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithEmptyEnvironmentVariable_IgnoresEmptyEntry()
        {
            // Arrange - Add an empty environment variable
            Environment.SetEnvironmentVariable("TENANT_INFO_EMPTY", "");

            // Recreate service to pick up new environment variable
            _service = new EnvironmentVariablesTenantService();

            // Act - Should still work with valid API keys
            TenantInfo? result = _service.GetTenantInfoByApiKey("api-key-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("tenant1", result.TenantId);

            // Clean up
            Environment.SetEnvironmentVariable("TENANT_INFO_EMPTY", null);
        }

        [TestMethod]
        public void GetTenantInfoByApiKey_WithWhitespaceEnvironmentVariable_IgnoresWhitespaceEntry()
        {
            // Arrange - Add a whitespace-only environment variable
            Environment.SetEnvironmentVariable("TENANT_INFO_WHITESPACE", "   ");

            // Recreate service to pick up new environment variable
            _service = new EnvironmentVariablesTenantService();

            // Act - Should still work with valid API keys
            TenantInfo? result = _service.GetTenantInfoByApiKey("api-key-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("tenant1", result.TenantId);

            // Clean up
            Environment.SetEnvironmentVariable("TENANT_INFO_WHITESPACE", null);
        }
    }
}