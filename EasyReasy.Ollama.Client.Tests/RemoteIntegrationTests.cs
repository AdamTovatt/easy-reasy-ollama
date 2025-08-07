using EasyReasy.Ollama.Common;

namespace EasyReasy.Ollama.Client.Tests
{
    [TestClass]
    public class RemoteIntegrationTests
    {
        private static IOllamaClient _client = null!;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("url");

            _client = await OllamaClient.CreateAuthorizedAsync(httpClient, "apikey");
        }

        [TestMethod]
        public async Task GetEmbeddingWorks()
        {
            const string text = "Hello world";
            EmbeddingResponse response = await _client.Embeddings.GetEmbeddingsAsync("nomic-embed-text", text);

            Console.WriteLine($"Embedding for '{text}': {string.Join(", ", response.Embeddings)}");
        }
    }
}
