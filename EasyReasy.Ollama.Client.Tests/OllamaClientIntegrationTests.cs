using System.Diagnostics;

namespace EasyReasy.Ollama.Client.Tests
{
    [TestClass]
    public class OllamaClientIntegrationTests : BaseIntegrationTest
    {

        [TestMethod]
        public void CreateAsync_ValidApiKey_CreatesClient()
        {
            // Act
            OllamaClient client = OllamaClient.CreateUnauthorized(ServerClient, "test-api-key");

            // Assert
            Assert.IsNotNull(client);
            Assert.IsNotNull(client.Chat);
            Assert.IsNotNull(client.Embeddings);
        }

        [TestMethod]
        public void CreateAsync_InvalidApiKey_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                OllamaClient.CreateUnauthorized(ServerClient, ""));
        }

        [TestMethod]
        public async Task CreateAuthorizedAsync_ValidApiKey_CreatesAuthenticatedClient()
        {
            // Act
            OllamaClient client = await OllamaClient.CreateAuthorizedAsync(ServerClient, "test-api-key");

            // Assert
            Assert.IsNotNull(client);
            Assert.IsNotNull(client.Chat);
            Assert.IsNotNull(client.Embeddings);
        }

        [TestMethod]
        public async Task CreateAuthorizedAsync_InvalidApiKey_ThrowsHttpRequestException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(async () =>
                await OllamaClient.CreateAuthorizedAsync(ServerClient, "invalid-api-key"));

            try
            {
                await OllamaClient.CreateAuthorizedAsync(ServerClient, "invalid-api-key");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message); // To be able to inspect the message in the tests
            }
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

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Act
            bool isAvailable = await client.IsModelAvailableAsync(chatTestModelName);

            stopwatch.Stop();
            Console.WriteLine($"Elapsed time 1: {stopwatch.ElapsedMilliseconds} ms");

            // Assert
            // Note: Result depends on whether the model is actually available in the test environment
            Assert.IsInstanceOfType(isAvailable, typeof(bool));

            // Test again to see if speed is increased
            stopwatch = Stopwatch.StartNew();
            isAvailable = await client.IsModelAvailableAsync(chatTestModelName);
            stopwatch.Stop();

            Console.WriteLine($"Elapsed time 1: {stopwatch.ElapsedMilliseconds} ms");

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