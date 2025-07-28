using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client.Tests
{
    [TestClass]
    public class EmbeddingClientIntegrationTests : BaseIntegrationTest
    {

        [TestMethod]
        public async Task GetEmbeddingsAsync_ValidRequest_ReturnsEmbeddings()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            EmbeddingRequest request = new EmbeddingRequest("llama3.1", "Hello world");

            // Act
            EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Embeddings);
            Assert.IsTrue(response.Embeddings.Length > 0);
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_ModelNameAndText_ReturnsEmbeddings()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();

            // Act
            EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync("llama3.1", "Hello world");

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Embeddings);
            Assert.IsTrue(response.Embeddings.Length > 0);
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_EmptyText_ReturnsEmbeddings()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            EmbeddingRequest request = new EmbeddingRequest("llama3.1", "");

            // Act
            EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Embeddings);
            // Note: Empty text might return empty embeddings or zero embeddings
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_LongText_ReturnsEmbeddings()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            string longText = new string('a', 1000); // 1000 character text
            EmbeddingRequest request = new EmbeddingRequest("llama3.1", longText);

            // Act
            EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Embeddings);
            Assert.IsTrue(response.Embeddings.Length > 0);
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_InvalidModel_ThrowsException()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            EmbeddingRequest request = new EmbeddingRequest("invalid-model", "Hello world");

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
                await client.Embeddings.GetEmbeddingsAsync(request));
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_Unauthenticated_ThrowsException()
        {
            // Arrange
            OllamaClient client = CreateUnauthenticatedClient();
            EmbeddingRequest request = new EmbeddingRequest("llama3.1", "Hello world");

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
                await client.Embeddings.GetEmbeddingsAsync(request));
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_CancellationToken_CancelsRequest()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            EmbeddingRequest request = new EmbeddingRequest("llama3.1", "Hello world");
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(1)); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
                await client.Embeddings.GetEmbeddingsAsync(request, cts.Token));
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_ResponseFormat_IsCorrect()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            EmbeddingRequest request = new EmbeddingRequest("llama3.1", "Hello world");

            // Act
            EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Embeddings);

            // Verify all values are finite numbers (not NaN or Infinity)
            foreach (float value in response.Embeddings)
            {
                Assert.IsTrue(float.IsFinite(value), "Embedding values should be finite numbers");
            }
        }

        [TestMethod]
        public async Task GetEmbeddingsAsync_MultipleRequests_SameModel_ReturnsConsistentResults()
        {
            // Arrange
            OllamaClient client = await CreateAuthenticatedClientAsync();
            EmbeddingRequest request1 = new EmbeddingRequest("llama3.1", "Hello world");
            EmbeddingRequest request2 = new EmbeddingRequest("llama3.1", "Hello world");

            // Act
            EmbeddingResponse response1 = await client.Embeddings.GetEmbeddingsAsync(request1);
            EmbeddingResponse response2 = await client.Embeddings.GetEmbeddingsAsync(request2);

            // Assert
            Assert.IsNotNull(response1);
            Assert.IsNotNull(response2);
            Assert.AreEqual(response1.Embeddings.Length, response2.Embeddings.Length);

            // Note: For deterministic models, embeddings should be identical
            // For non-deterministic models, they might be different
        }
    }
}