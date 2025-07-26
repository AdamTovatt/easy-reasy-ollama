using EasyReasy.Ollama.Server.Providers;

namespace EasyReasy.Ollama.Server.Tests
{
    [TestClass]
    public class EnvironmentVariablesAllowedModelsProviderTests
    {
        private EnvironmentVariablesAllowedModelsProvider _provider = null!;

        [TestInitialize]
        public void Setup()
        {
            // Set up test environment variables
            Environment.SetEnvironmentVariable("ALLOWED_MODEL1", "llama2");
            Environment.SetEnvironmentVariable("ALLOWED_MODEL2", "codellama");
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_DEV", "llama2:7b");
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_PROD", "llama2:13b");

            _provider = new EnvironmentVariablesAllowedModelsProvider();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test environment variables
            Environment.SetEnvironmentVariable("ALLOWED_MODEL1", null);
            Environment.SetEnvironmentVariable("ALLOWED_MODEL2", null);
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_DEV", null);
            Environment.SetEnvironmentVariable("ALLOWED_MODEL_PROD", null);
        }

        [TestMethod]
        public void GetAllowedModels_ReturnsModelsFromEnvironmentVariables()
        {
            // Act
            IEnumerable<string> result = _provider.GetAllowedModels();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(), "Should return at least one model from environment variables");

            // Verify specific models are included
            List<string> modelsList = result.ToList();
            Assert.IsTrue(modelsList.Contains("llama2"), "Should include llama2");
            Assert.IsTrue(modelsList.Contains("codellama"), "Should include codellama");
            Assert.IsTrue(modelsList.Contains("llama2:7b"), "Should include llama2:7b");
            Assert.IsTrue(modelsList.Contains("llama2:13b"), "Should include llama2:13b");
        }

        [TestMethod]
        public void IsModelAllowed_WithValidModel_ReturnsTrue()
        {
            // Arrange
            string validModel = "llama2";

            // Act
            bool result = _provider.IsModelAllowed(validModel);

            // Assert
            Assert.IsTrue(result, $"Model '{validModel}' should be allowed");
        }

        [TestMethod]
        public void IsModelAllowed_WithInvalidModel_ReturnsFalse()
        {
            // Arrange
            string invalidModel = "non-existent-model-12345";

            // Act
            bool result = _provider.IsModelAllowed(invalidModel);

            // Assert
            Assert.IsFalse(result, $"Model '{invalidModel}' should not be allowed");
        }

        [TestMethod]
        public void IsModelAllowed_WithNullModel_ReturnsFalse()
        {
            // Act
            bool result = _provider.IsModelAllowed(null!);

            // Assert
            Assert.IsFalse(result, "Null model should not be allowed");
        }

        [TestMethod]
        public void IsModelAllowed_WithEmptyModel_ReturnsFalse()
        {
            // Act
            bool result = _provider.IsModelAllowed("");

            // Assert
            Assert.IsFalse(result, "Empty model should not be allowed");
        }

        [TestMethod]
        public void IsModelAllowed_WithWhitespaceModel_ReturnsFalse()
        {
            // Act
            bool result = _provider.IsModelAllowed("   ");

            // Assert
            Assert.IsFalse(result, "Whitespace-only model should not be allowed");
        }

        [TestMethod]
        public void IsModelAllowed_WithCaseInsensitiveMatch_ReturnsTrue()
        {
            // Arrange
            string validModel = "llama2";
            string upperCaseModel = validModel.ToUpper();
            string lowerCaseModel = validModel.ToLower();

            // Act & Assert
            bool upperCaseResult = _provider.IsModelAllowed(upperCaseModel);
            bool lowerCaseResult = _provider.IsModelAllowed(lowerCaseModel);

            Assert.IsTrue(upperCaseResult, $"Uppercase model '{upperCaseModel}' should be allowed");
            Assert.IsTrue(lowerCaseResult, $"Lowercase model '{lowerCaseModel}' should be allowed");
        }

        [TestMethod]
        public void IsModelAllowed_CachesResults()
        {
            // Arrange
            string testModel = "codellama";

            // Act - First call should populate cache
            bool firstResult = _provider.IsModelAllowed(testModel);

            // Clear cache to verify it was used
            _provider.ClearCache();

            // Second call should use fresh cache
            bool secondResult = _provider.IsModelAllowed(testModel);

            // Assert
            Assert.IsTrue(firstResult, "First call should return true for valid model");
            Assert.IsTrue(secondResult, "Second call should return true for valid model");
        }

        [TestMethod]
        public void ClearCache_RemovesCachedResults()
        {
            // Arrange
            string testModel = "llama2:7b";

            // Act - First call to populate cache
            _provider.IsModelAllowed(testModel);

            // Clear cache
            _provider.ClearCache();

            // Call again - should work the same but with fresh cache
            bool result = _provider.IsModelAllowed(testModel);

            // Assert
            Assert.IsTrue(result, "Should still return true after cache clear");
        }

        [TestMethod]
        public void GetAllowedModels_ReturnsSameResultOnMultipleCalls()
        {
            // Act
            IEnumerable<string> firstCall = _provider.GetAllowedModels();
            IEnumerable<string> secondCall = _provider.GetAllowedModels();

            // Assert
            Assert.IsNotNull(firstCall);
            Assert.IsNotNull(secondCall);
            CollectionAssert.AreEqual(firstCall.ToList(), secondCall.ToList(), "Multiple calls should return the same results");
        }
    }
}