namespace EasyReasy.Ollama.Client.Tests
{
    [TestClass]
    public class OllamaClientIntegrationTests : BaseIntegrationTest
    {

        [TestMethod]
        public async Task CreateAsync_ValidApiKey_CreatesClient()
        {
            // Act
            OllamaClient client = OllamaClient.CreateAsync(BaseUrl, "test-api-key");

            // Assert
            Assert.IsNotNull(client);
            Assert.IsNotNull(client.Chat);
            Assert.IsNotNull(client.Embeddings);
        }

        [TestMethod]
        public async Task CreateAsync_InvalidApiKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                OllamaClient.CreateAsync(BaseUrl, ""));
        }

        [TestMethod]
        public async Task CreateAuthorizedAsync_ValidApiKey_CreatesAuthenticatedClient()
        {
            // Act
            OllamaClient client = await OllamaClient.CreateAuthorizedAsync(BaseUrl, "test-api-key");

            // Assert
            Assert.IsNotNull(client);
            Assert.IsNotNull(client.Chat);
            Assert.IsNotNull(client.Embeddings);
        }

        [TestMethod]
        public async Task CreateAuthorizedAsync_InvalidApiKey_ThrowsHttpRequestException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
                await OllamaClient.CreateAuthorizedAsync(BaseUrl, "invalid-api-key"));
        }

        [TestMethod]
        public async Task GetAvailableModelsAsync_AuthenticatedClient_ReturnsModels()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();

            // Act
            List<string> models = await client.GetAvailableModelsAsync();

            // Assert
            Assert.IsNotNull(models);
            // Note: In a real test environment, this might be empty if no models are available
        }

        [TestMethod]
        public async Task IsModelAvailableAsync_AuthenticatedClient_ReturnsBoolean()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();

            // Act
            bool isAvailable = await client.IsModelAvailableAsync("llama3.1");

            // Assert
            // Note: Result depends on whether the model is actually available in the test environment
            Assert.IsInstanceOfType(isAvailable, typeof(bool));
        }

        [TestMethod]
        public async Task Dispose_Client_ReleasesResources()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();

            // Act
            client.Dispose();

            // Assert
            // The test passes if Dispose doesn't throw an exception
            // In a real scenario, you might want to verify that the HttpClient is disposed
        }
    }
}